using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.ViewModels.Controls
{
    public class LayerButtonViewModel : ViewModelBase
    {
        public ICommand ButtonClick { get; }
        public string ButtonText
        {
            get => _buttonText;
            set => this.RaiseAndSetIfChanged(ref _buttonText, value);
        }

        private TimelineViewModel _parentTimelineViewModel;
        private LayerObject targetLayerObject;

        private string _buttonText = "Layer";
        public LayerButtonViewModel(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject) 
        {
            _parentTimelineViewModel = parentTimelineViewModel;
            this.targetLayerObject = targetLayerObject;

            ButtonClick = ReactiveCommand.Create(() =>
            {
                var command = new LayerIsActiveChangeCommand(targetLayerObject, !targetLayerObject.IsActive);
                _parentTimelineViewModel.RunEditCommand(command);
            });

            ButtonText = targetLayerObject.Name;
        }
    }
}
