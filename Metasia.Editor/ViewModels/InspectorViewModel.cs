using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.ViewModels.Inspector;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Inspector;

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

        private readonly IEditCommandManager _editCommandManager;
        private string _testCharacters = String.Empty;
        private readonly ISelectionState _selectionState;
        private readonly IProjectState _projectState;
        private readonly IClipSettingPaneViewModelFactory _clipSettingPaneViewModelFactory;
        public InspectorViewModel(ISelectionState selectionState, IProjectState projectState, IEditCommandManager editCommandManager, IClipSettingPaneViewModelFactory clipSettingPaneViewModelFactory)
        {
            ArgumentNullException.ThrowIfNull(selectionState);
            ArgumentNullException.ThrowIfNull(projectState);
            ArgumentNullException.ThrowIfNull(editCommandManager);
            ArgumentNullException.ThrowIfNull(clipSettingPaneViewModelFactory);
            _selectionState = selectionState;
            _projectState = projectState;
            _editCommandManager = editCommandManager;
            _clipSettingPaneViewModelFactory = clipSettingPaneViewModelFactory;
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
                        var clipSettingPaneViewModel = _clipSettingPaneViewModelFactory.Create();
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

