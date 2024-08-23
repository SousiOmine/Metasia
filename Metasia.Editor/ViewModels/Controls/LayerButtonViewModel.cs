using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private LayerObject targetLayerObject;

        private string _buttonText = "Layer";
        public LayerButtonViewModel(LayerObject targetLayerObject) 
        {
            this.targetLayerObject = targetLayerObject;

            ButtonClick = ReactiveCommand.Create(() =>
            {
                targetLayerObject.IsActive = !targetLayerObject.IsActive;
            });

            ButtonText = targetLayerObject.Name;
        }
    }
}
