using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.ViewModels.Inspector;

namespace Metasia.Editor.ViewModels
{
    public class InspectorViewModel : ViewModelBase
    {
        public ClipSettingPaneViewModel ClipSettingPane { get; }
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

            ClipSettingPane = new ClipSettingPaneViewModel();
        }

        public void PlayerChanged()
        {
            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                _playerParentViewModel.TargetPlayerViewModel.SelectingObjects.CollectionChanged += (sender, args) =>
                {
                    if (_playerParentViewModel.TargetPlayerViewModel.SelectingObjects.Count > 0)
                    {
                        ClipSettingPane.TargetObject = _playerParentViewModel.TargetPlayerViewModel.SelectingObjects.FirstOrDefault();
                    }
                    else
                    {
                        ClipSettingPane.TargetObject = null;
                    }
                };
            }
        }
    }
}

