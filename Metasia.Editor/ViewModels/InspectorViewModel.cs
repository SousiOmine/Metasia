using System;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class InspectorViewModel : ViewModelBase
    {
        public string TestCharacters
        {
            get => _testCharacters;
            set => this.RaiseAndSetIfChanged(ref _testCharacters, value);
        }

        private string _testCharacters = String.Empty;
        public InspectorViewModel(PlayerViewModel playerViewModel)
        {
            
            playerViewModel.TargetObjects.CollectionChanged += (sender, args) =>
            {
                if (playerViewModel.TargetObjects.Count > 0)
                {
                    TestCharacters = playerViewModel.TargetObjects[0].Id;
                }
                else
                {
                    TestCharacters = String.Empty;
                }
            };
        }
    }
}

