using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Timeline
{
    public class LayerButtonViewModel : ViewModelBase
    {
        private readonly LayerObject _targetLayerObject;
        private readonly IEditCommandManager _editCommandManager;
        private readonly IProjectState _projectState;
        private readonly ISelectionState _selectionState;
        private bool _disposed;

        public ICommand ButtonClick { get; }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        private bool _isActive = true;

        public bool IsSelected
        {
            get => _isSelected;
            private set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        private bool _isSelected = false;

        public string ButtonText
        {
            get => _buttonText;
            set => this.RaiseAndSetIfChanged(ref _buttonText, value);
        }

        private string _buttonText = "Layer";

        public LayerButtonViewModel(LayerObject targetLayerObject, IEditCommandManager editCommandManager, IProjectState projectState, ISelectionState selectionState)
        {
            _targetLayerObject = targetLayerObject;
            _editCommandManager = editCommandManager;
            _projectState = projectState;
            _selectionState = selectionState;

            ButtonClick = ReactiveCommand.Create(() =>
            {
                var command = new LayerIsActiveChangeCommand(_targetLayerObject, !_targetLayerObject.IsActive);
                _editCommandManager.Execute(command);
            });

            ButtonText = targetLayerObject.Name;
            IsActive = targetLayerObject.IsActive;

            _projectState.TimelineChanged += OnTimelineChanged;
            _selectionState.LayerSelectionChanged += OnLayerSelectionChanged;
            UpdateIsSelected();
        }

        private void OnTimelineChanged()
        {
            IsActive = _targetLayerObject.IsActive;
        }

        private void OnLayerSelectionChanged()
        {
            UpdateIsSelected();
        }

        private void UpdateIsSelected()
        {
            IsSelected = _selectionState.SelectedLayer?.Id == _targetLayerObject.Id;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _projectState.TimelineChanged -= OnTimelineChanged;
                    _selectionState.LayerSelectionChanged -= OnLayerSelectionChanged;
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
