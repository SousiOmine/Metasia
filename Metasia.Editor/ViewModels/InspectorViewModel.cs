using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

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

        private IEditCommandManager _editCommandManager;
        private string _testCharacters = String.Empty;
        private ISelectionState _selectionState;
        private IProjectState _projectState;
        public InspectorViewModel(ISelectionState selectionState, IProjectState projectState, IEditCommandManager editCommandManager)
        {
            _selectionState = selectionState;
            _projectState = projectState;
            _editCommandManager = editCommandManager;
            PlayerChanged();

            _projectState.ProjectLoaded += () =>
            {
                TestCharacters = string.Empty;
                PlayerChanged();
            };
        }

        public void PlayerChanged()
        {
            if (_projectState.CurrentProject is not null)
            {
                _selectionState.SelectionChanged += () =>
                {
                    if (_selectionState.SelectedClips.Count > 0)
                    {
                        ClipSettingPanes.Clear();
                        var clipSettingPaneViewModel = new ClipSettingPaneViewModel(this);
                        clipSettingPaneViewModel.TargetObject = _selectionState.SelectedClips.FirstOrDefault();
                        ClipSettingPanes.Add(clipSettingPaneViewModel);
                    }
                    else
                    {
                        ClipSettingPanes.Clear();
                    }
                };
            }
        }

        public void RunEditCommand(IEditCommand editCommand)
        {
            _editCommandManager.Execute(editCommand);
        }
    }
}

