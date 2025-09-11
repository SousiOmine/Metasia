using System;
using System.Diagnostics;
using System.Windows.Input;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using System.Threading.Tasks;
using Metasia.Editor.Models;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Models.States;

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

        public ICommand Undo { get; }
        public ICommand Redo { get; }

        private IFileDialogService _fileDialogService;
        private IProjectState _projectState;
        
        public MainWindowViewModel(
            PlayerParentViewModel playerParentVM,
            TimelineParentViewModel timelineParentVM,
            InspectorViewModel inspectorViewModel,
            ToolsViewModel toolsVM,
            IKeyBindingService keyBindingService,
            IFileDialogService fileDialogService,
            IProjectState projectState)
        {
            PlayerParentVM = playerParentVM;
            TimelineParentVM = timelineParentVM;
            InspectorVM = inspectorViewModel;
            ToolsVM = toolsVM;
            _fileDialogService = fileDialogService;
            _projectState = projectState;
            LoadEditingProject = ReactiveCommand.Create(LoadEditingProjectExecuteAsync);
            CreateNewProject = ReactiveCommand.Create(CreateNewProjectExecuteAsync);
            OverrideSaveEditingProject = ReactiveCommand.Create(OverrideSaveEditingProjectExecuteAsync);

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
            }
        }

        private async Task CreateNewProjectExecuteAsync()
        {
            try
            {
                var newProjectDialogService = App.Current?.Services?.GetService<INewProjectDialogService>();
                if (newProjectDialogService is null)
                {
                    Debug.WriteLine("NewProjectDialogService is not found");
                    return;
                }

                var (result, projectPath, projectInfo, selectedTemplate) = await newProjectDialogService.ShowDialogAsync();

                if (result)
                {
                    MetasiaEditorProject editorProject = ProjectGenerator.CreateProject(projectPath, projectInfo, selectedTemplate);
                    await _projectState.LoadProjectAsync(editorProject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"新規プロジェクト作成エラー: {ex.Message}");
            }
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
    }
}
