using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Timeline
{
    public class LayerButtonViewModel : ViewModelBase
    {
        private readonly LayerObject _targetLayerObject;
        private readonly IEditCommandManager _editCommandManager;
        private readonly IProjectState _projectState;
        private bool _disposed;

        public ICommand ButtonClick { get; }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        private bool _isActive = true;

        public string ButtonText
        {
            get => _buttonText;
            set => this.RaiseAndSetIfChanged(ref _buttonText, value);
        }

        private string _buttonText = "Layer";

        public LayerButtonViewModel(LayerObject targetLayerObject, IEditCommandManager editCommandManager, IProjectState projectState)
        {
            _targetLayerObject = targetLayerObject;
            _editCommandManager = editCommandManager;
            _projectState = projectState;

            ButtonClick = ReactiveCommand.Create(() =>
            {
                var command = new LayerIsActiveChangeCommand(targetLayerObject, !targetLayerObject.IsActive);
                editCommandManager.Execute(command);
            });

            ButtonText = targetLayerObject.Name;
            IsActive = targetLayerObject.IsActive;

            _projectState.TimelineChanged += OnTimelineChanged;
        }

        private void OnTimelineChanged()
        {
            IsActive = _targetLayerObject.IsActive;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _projectState.TimelineChanged -= OnTimelineChanged;
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
