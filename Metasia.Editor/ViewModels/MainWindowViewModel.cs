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
    public class MainWindowViewModel : ViewModelBase
    {
        public PlayerParentViewModel PlayerParentVM { get; }

        public InspectorViewModel InspectorVM { get; }

        public TimelineParentViewModel TimelineParentVM { get; }

        public ToolsViewModel ToolsVM { get; }

        public ICommand LoadEditingProject { get; }
        public ICommand CreateNewProject { get; }
        public ICommand OverrideSaveEditingProject { get; }
        public ICommand OpenSettings { get; }

        public ICommand Undo { get; }
        public ICommand Redo { get; }

        public ICommand SetTimelineSelectionStart { get; }
        public ICommand SetTimelineSelectionEnd { get; }
        public ICommand ClearTimelineSelection { get; }

        public Interaction<NewProjectViewModel, (bool Result, string ProjectPath, Metasia.Core.Project.ProjectInfo ProjectInfo, Metasia.Core.Project.MetasiaProject? SelectedTemplate)> NewProjectInteraction { get; } = new();
        public Interaction<Unit, Unit> OpenSettingsInteraction { get; } = new();

        private readonly IFileDialogService _fileDialogService;
        private readonly IProjectState _projectState;

        private readonly IPlaybackState _playbackState;
        private readonly INewProjectViewModelFactory _newProjectViewModelFactory;
        private readonly IEditCommandManager _editCommandManager;

        public MainWindowViewModel(
            PlayerParentViewModel playerParentVM,
            TimelineParentViewModel timelineParentVM,
            InspectorViewModel inspectorViewModel,
            ToolsViewModel toolsVM,
            IKeyBindingService keyBindingService,
            IFileDialogService fileDialogService,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            INewProjectViewModelFactory newProjectViewModelFactory)
        {
            PlayerParentVM = playerParentVM;
            TimelineParentVM = timelineParentVM;
            InspectorVM = inspectorViewModel;
            ToolsVM = toolsVM;
            _fileDialogService = fileDialogService;
            _projectState = projectState;
            _playbackState = playbackState;
            _newProjectViewModelFactory = newProjectViewModelFactory;
            _editCommandManager = editCommandManager;
            LoadEditingProject = ReactiveCommand.Create(LoadEditingProjectExecuteAsync);
            CreateNewProject = ReactiveCommand.Create(CreateNewProjectExecuteAsync);
            OverrideSaveEditingProject = ReactiveCommand.Create(OverrideSaveEditingProjectExecuteAsync);
            OpenSettings = ReactiveCommand.CreateFromTask(OpenSettingsExecuteAsync);
            SetTimelineSelectionStart = ReactiveCommand.Create(SetTimelineSelectionStartMethod);
            SetTimelineSelectionEnd = ReactiveCommand.Create(SetTimelineSelectionEndMethod);
            ClearTimelineSelection = ReactiveCommand.Create(ClearTimelineSelectionMethod);

            Undo = ReactiveCommand.Create(UndoExecute);
            Redo = ReactiveCommand.Create(RedoExecute);

            // キーバインディングサービスにコマンドを登録
            RegisterCommands(keyBindingService);
        }

        /// <summary>
        /// キーバインディングサービスにコマンドを登録
        /// </summary>
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
                    ProjectSaveLoadManager.Save(_projectState.CurrentProject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロジェクト上書き保存エラー: {ex.Message}");
            }
        }

        private async Task LoadEditingProjectExecuteAsync()
        {
            var folder = await _fileDialogService.OpenFolderDialogAsync();
            if (folder is null) return;

            MetasiaEditorProject editorProject = ProjectSaveLoadManager.Load(new DirectoryEntity(folder.Path.LocalPath));
            await _projectState.LoadProjectAsync(editorProject);
        }

        private void UndoExecute()
        {
            if (PlayerParentVM.TargetPlayerViewModel is not null)
            {
                PlayerParentVM.TryUndo();
            }
        }

        private void RedoExecute()
        {
            if (PlayerParentVM.TargetPlayerViewModel is not null)
            {
                PlayerParentVM.TryRedo();
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
    }
}
