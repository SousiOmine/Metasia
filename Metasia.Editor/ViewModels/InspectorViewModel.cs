using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.ViewModels.Inspector;

namespace Metasia.Editor.ViewModels
{
    public class InspectorViewModel : ViewModelBase
    {
        public ObservableCollection<ClipSettingPaneViewModel> ClipSettingPanes { get; } = new();
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
                        ClipSettingPanes.Clear();
                        foreach (var obj in _playerParentViewModel.TargetPlayerViewModel.SelectingObjects)
                        {
                            ClipSettingPanes.Add(new ClipSettingPaneViewModel(obj));
                        }
                        TestCharacters = string.Join(", ", _playerParentViewModel.TargetPlayerViewModel.SelectingObjects.Select(obj => obj.Id));
                    }
                    else
                    {
                        ClipSettingPanes.Clear();
                        TestCharacters = string.Empty;
                    }
                };
            }
        }
    }
}

