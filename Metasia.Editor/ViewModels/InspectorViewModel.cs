using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels
{
    public class InspectorViewModel : ViewModelBase
    {
        public ObservableCollection<ClipSettingPaneViewModel> ClipSettingPanes { get; set; } = new();
        public string TestCharacters
        {
            get => _testCharacters;
            set => this.RaiseAndSetIfChanged(ref _testCharacters, value);
        }

        private PlayerParentViewModel _playerParentViewModel;
        private PlayerViewModel _playerViewModel;
        private string _testCharacters = String.Empty;
        private ClipObject? _targetObject;

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
                        var clipSettingPaneViewModel = new ClipSettingPaneViewModel(this);
                        clipSettingPaneViewModel.TargetObject = _playerParentViewModel.TargetPlayerViewModel.SelectingObjects.FirstOrDefault();
                        ClipSettingPanes.Add(clipSettingPaneViewModel);
                    }
                    else
                    {
                        ClipSettingPanes.Clear();
                    }
                };
                _playerViewModel = _playerParentViewModel.TargetPlayerViewModel;
            }
        }

        public void RunEditCommand(IEditCommand editCommand)
        {
            _playerViewModel.RunEditCommand(editCommand);
        }
    }
}

