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

        private PlayerParentViewModel _playerParentViewModel;
        private string _testCharacters = String.Empty;
        public InspectorViewModel(PlayerParentViewModel playerParentViewModel)
        {
            _playerParentViewModel = playerParentViewModel;
            PlayerChanged();

            _playerParentViewModel.ProjectInstanceChanged += (sender, args) =>
            {
                TestCharacters = string.Empty;
                PlayerChanged();
            };
        }

        public void PlayerChanged()
        {
            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                _playerParentViewModel.TargetPlayerViewModel.SelectingObjects.CollectionChanged += (sender, args) =>
                {
                    if (_playerParentViewModel.TargetPlayerViewModel.SelectingObjects.Count > 0)
                    {
                        TestCharacters = _playerParentViewModel.TargetPlayerViewModel.SelectingObjects[0].Id;
                    }
                    else
                    {
                        TestCharacters = string.Empty;
                    }
                };
            }
        }
    }
}

