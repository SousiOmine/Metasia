using System;
using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Metasia.Editor.Models;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Services;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ICommand LoadEditingProject { get; }
        public ICommand CreateNewProject { get; }
        public ICommand OverrideSaveEditingProject { get; }
        public ICommand OpenSettings { get; }

        public ICommand Undo { get; }
        public ICommand Redo { get; }

        public ICommand Copy { get; }
        public ICommand Paste { get; }
        public ICommand Cut { get; }

        public ICommand SetTimelineSelectionStart { get; }
        public ICommand SetTimelineSelectionEnd { get; }
        public ICommand ClearTimelineSelection { get; }
        public ICommand OpenOutput { get; }

        public Interaction<NewProjectViewModel, (bool Result, string ProjectPath, Metasia.Core.Project.ProjectInfo ProjectInfo, Metasia.Core.Project.MetasiaProject? SelectedTemplate)> NewProjectInteraction { get; } = new();
        public Interaction<OutputViewModel, object> OutputInteraction { get; } = new();
        public Interaction<Unit, Unit> OpenSettingsInteraction { get; } = new();

        private readonly IFileDialogService _fileDialogService;
        private readonly IProjectState _projectState;
        private readonly IPlaybackState _playbackState;
        private readonly INewProjectViewModelFactory _newProjectViewModelFactory;
        private readonly IEditCommandManager _editCommandManager;
        private readonly IOutputViewModelFactory _outputViewModelFactory;
        private readonly PlayerParentViewModel _playerParentViewModel;
        private readonly TimelineParentViewModel _timelineParentViewModel;

        public MenuViewModel(
            PlayerParentViewModel playerParentViewModel,
            TimelineParentViewModel timelineParentViewModel,
            IKeyBindingService keyBindingService,
            IFileDialogService fileDialogService,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            INewProjectViewModelFactory newProjectViewModelFactory,
            IOutputViewModelFactory outputViewModelFactory)
        {
            _playerParentViewModel = playerParentViewModel;
            _timelineParentViewModel = timelineParentViewModel;
            _fileDialogService = fileDialogService;
            _projectState = projectState;
            _playbackState = playbackState;
            _newProjectViewModelFactory = newProjectViewModelFactory;
            _outputViewModelFactory = outputViewModelFactory;
            _editCommandManager = editCommandManager;

            LoadEditingProject = ReactiveCommand.CreateFromTask(LoadEditingProjectExecuteAsync);
            CreateNewProject = ReactiveCommand.CreateFromTask(CreateNewProjectExecuteAsync);
            OverrideSaveEditingProject = ReactiveCommand.CreateFromTask(OverrideSaveEditingProjectExecuteAsync);
            OpenSettings = ReactiveCommand.CreateFromTask(OpenSettingsExecuteAsync);
            SetTimelineSelectionStart = ReactiveCommand.Create(SetTimelineSelectionStartMethod);
            SetTimelineSelectionEnd = ReactiveCommand.Create(SetTimelineSelectionEndMethod);
            ClearTimelineSelection = ReactiveCommand.Create(ClearTimelineSelectionMethod);
            OpenOutput = ReactiveCommand.CreateFromTask(OpenOutputExecuteAsync);

            Undo = ReactiveCommand.Create(UndoExecute);
            Redo = ReactiveCommand.Create(RedoExecute);

            Copy = ReactiveCommand.Create(() => _timelineParentViewModel.Copy());
            Paste = ReactiveCommand.Create(() => _timelineParentViewModel.Paste());
            Cut = ReactiveCommand.Create(() => _timelineParentViewModel.Cut());

            RegisterCommands(keyBindingService);
        }

        private void RegisterCommands(IKeyBindingService keyBindingService)
        {
            if (keyBindingService is not null)
            {
                keyBindingService.RegisterCommand("Undo", Undo);
                keyBindingService.RegisterCommand("Redo", Redo);
                keyBindingService.RegisterCommand("LoadEditingProject", LoadEditingProject);
                keyBindingService.RegisterCommand("CreateNewProject", CreateNewProject);
                keyBindingService.RegisterCommand("OverrideSaveEditingProject", OverrideSaveEditingProject);
                keyBindingService.RegisterCommand("OpenSettings", OpenSettings);
                keyBindingService.RegisterCommand("Copy", Copy);
                keyBindingService.RegisterCommand("Paste", Paste);
                keyBindingService.RegisterCommand("Cut", Cut);
            }
        }

        private async Task CreateNewProjectExecuteAsync()
        {
            try
            {
                var vm = _newProjectViewModelFactory.Create();
                var result = await NewProjectInteraction.Handle(vm).FirstAsync();

                if (result.Result)
                {
                    MetasiaEditorProject editorProject = ProjectGenerator.CreateProject(result.ProjectPath, result.ProjectInfo, result.SelectedTemplate);
                    await _projectState.LoadProjectAsync(editorProject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"新規プロジェクト作成エラー: {ex.Message}");
            }
        }

        private async Task OpenSettingsExecuteAsync()
        {
            await OpenSettingsInteraction.Handle(Unit.Default);
        }

        private async Task OverrideSaveEditingProjectExecuteAsync()
        {
            try
            {
                if (_projectState.CurrentProject is not null)
                {
                    string? targetFilePath = _projectState.CurrentProject.ProjectFilePath;
                    if (string.IsNullOrEmpty(_projectState.CurrentProject.ProjectFilePath))
                    {
                        var file = await _fileDialogService.SaveFileDialogAsync(
                            "プロジェクトを保存",
                            ["*.mtpj"],
                            "mtpj");
                        if (file is null) return;

                        targetFilePath = file.Path.LocalPath;
                    }

                    if (string.IsNullOrEmpty(targetFilePath)) return;

                    ProjectSaveLoadManager.Save(_projectState.CurrentProject, targetFilePath);

                    if (string.IsNullOrEmpty(_projectState.CurrentProject.ProjectFilePath))
                    {
                        _projectState.CurrentProject.ProjectFilePath = targetFilePath;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロジェクト上書き保存エラー: {ex.Message}");
            }
        }

        private async Task LoadEditingProjectExecuteAsync()
        {
            var file = await _fileDialogService.OpenFileDialogAsync("プロジェクトを開く", new[] { "*.mtpj" });
            if (file is null) return;

            try
            {
                string filePath = file.Path.LocalPath;
                MetasiaEditorProject editorProject = ProjectSaveLoadManager.Load(filePath);
                editorProject.ProjectFilePath = filePath;
                await _projectState.LoadProjectAsync(editorProject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロジェクト読込エラー: {ex.Message}");
            }
        }

        private void UndoExecute()
        {
            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                _playerParentViewModel.TryUndo();
            }
        }

        private void RedoExecute()
        {
            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                _playerParentViewModel.TryRedo();
            }
        }

        private void SetTimelineSelectionStartMethod()
        {
            if (_projectState.CurrentTimeline is not null)
            {
                var command = new TimelineSelectionRangeChangeCommand(_projectState.CurrentTimeline, _playbackState.CurrentFrame, _projectState.CurrentTimeline.SelectionEnd);
                _editCommandManager.Execute(command);
            }
        }

        private void SetTimelineSelectionEndMethod()
        {
            if (_projectState.CurrentTimeline is not null)
            {
                var command = new TimelineSelectionRangeChangeCommand(_projectState.CurrentTimeline, _projectState.CurrentTimeline.SelectionStart, _playbackState.CurrentFrame);
                _editCommandManager.Execute(command);
            }
        }

        private void ClearTimelineSelectionMethod()
        {
            if (_projectState.CurrentTimeline is not null)
            {
                var command = new TimelineSelectionRangeChangeCommand(_projectState.CurrentTimeline, 0, TimelineObject.MAX_LENGTH);
                _editCommandManager.Execute(command);
            }
        }

        private async Task OpenOutputExecuteAsync()
        {
            try
            {
                var vm = _outputViewModelFactory.Create();
                await OutputInteraction.Handle(vm).FirstAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"出力ウィンドウオープンエラー: {ex.Message}");
            }
        }
    }
}