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

namespace Metasia.Editor.ViewModels.Timeline
{
    public class LayerButtonViewModel : ViewModelBase
    {
        public ICommand ButtonClick { get; }
        public string ButtonText
        {
            get => _buttonText;
            set => this.RaiseAndSetIfChanged(ref _buttonText, value);
        }

        private string _buttonText = "Layer";
        public LayerButtonViewModel(LayerObject targetLayerObject, IEditCommandManager editCommandManager) 
        {

            ButtonClick = ReactiveCommand.Create(() =>
            {
                var command = new LayerIsActiveChangeCommand(targetLayerObject, !targetLayerObject.IsActive);
                editCommandManager.Execute(command);
            });

            ButtonText = targetLayerObject.Name;
        }
    }
}
