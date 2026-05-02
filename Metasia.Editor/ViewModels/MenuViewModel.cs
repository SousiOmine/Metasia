using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Metasia.Editor.Models;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Services;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Core.Objects;
using Metasia.Editor.Plugin;
using Avalonia.Controls;
using Metasia.Editor.ViewModels.Notifications;

namespace Metasia.Editor.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ICommand LoadEditingProject { get; }
        public ICommand CreateNewProject { get; }
        public ICommand SaveEditingProject { get; }
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
        public ICommand OpenPluginList { get; }
        public ICommand OpenPluginFolder { get; }
        public ICommand OpenNotifications { get; }
        public ICommand OpenAbout { get; }
        public ICommand Exit { get; }

        public ObservableCollection<object> SettingsMenuItems { get; }

        public Interaction<NewProjectViewModel, (bool Result, Metasia.Core.Project.ProjectInfo ProjectInfo, Metasia.Core.Project.MetasiaProject? SelectedTemplate)> NewProjectInteraction { get; } = new();
        public Interaction<OutputViewModel, object> OutputInteraction { get; } = new();
        public Interaction<Unit, Unit> OpenSettingsInteraction { get; } = new();
        public Interaction<PluginListViewModel, Unit> PluginListInteraction { get; } = new();
        public Interaction<IPluginSettingsProvider, Unit> OpenPluginSettingsInteraction { get; } = new();
        public Interaction<NotificationCenterViewModel, Unit> OpenNotificationsInteraction { get; } = new();
        public Interaction<Unit, Unit> AboutInteraction { get; } = new();
        public Interaction<Unit, Unit> ExitInteraction { get; } = new();

        private readonly IFileDialogService _fileDialogService;
        private readonly IProjectState _projectState;
        private readonly IPlaybackState _playbackState;
        private readonly INewProjectViewModelFactory _newProjectViewModelFactory;
        private readonly IEditCommandManager _editCommandManager;
        private readonly IOutputViewModelFactory _outputViewModelFactory;
        private readonly PlayerParentViewModel _playerParentViewModel;
        private readonly TimelineParentViewModel _timelineParentViewModel;
        private readonly IPluginService _pluginService;
        private readonly INotificationService _notificationService;
        private readonly NotificationCenterViewModel _notificationCenterViewModel;

        public MenuViewModel(
            PlayerParentViewModel playerParentViewModel,
            TimelineParentViewModel timelineParentViewModel,
            IKeyBindingService keyBindingService,
            IFileDialogService fileDialogService,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            INewProjectViewModelFactory newProjectViewModelFactory,
            IOutputViewModelFactory outputViewModelFactory,
            IPluginService pluginService,
            INotificationService notificationService,
            NotificationCenterViewModel notificationCenterViewModel)
        {
            _playerParentViewModel = playerParentViewModel;
            _timelineParentViewModel = timelineParentViewModel;
            _fileDialogService = fileDialogService;
            _projectState = projectState;
            _playbackState = playbackState;
            _newProjectViewModelFactory = newProjectViewModelFactory;
            _outputViewModelFactory = outputViewModelFactory;
            _editCommandManager = editCommandManager;
            _pluginService = pluginService;
            _notificationService = notificationService;
            _notificationCenterViewModel = notificationCenterViewModel;

            LoadEditingProject = ReactiveCommand.CreateFromTask(LoadEditingProjectExecuteAsync);
            CreateNewProject = ReactiveCommand.CreateFromTask(CreateNewProjectExecuteAsync);
            SaveEditingProject = ReactiveCommand.CreateFromTask(SaveEditingProjectExecuteAsync);
            OpenSettings = ReactiveCommand.CreateFromTask(OpenSettingsExecuteAsync);
            SetTimelineSelectionStart = ReactiveCommand.Create(SetTimelineSelectionStartMethod);
            SetTimelineSelectionEnd = ReactiveCommand.Create(SetTimelineSelectionEndMethod);
            ClearTimelineSelection = ReactiveCommand.Create(ClearTimelineSelectionMethod);
            OpenOutput = ReactiveCommand.CreateFromTask(OpenOutputExecuteAsync);
            OpenPluginList = ReactiveCommand.CreateFromTask(OpenPluginListExecuteAsync);
            OpenPluginFolder = ReactiveCommand.Create(OpenPluginFolderExecute);
            OpenNotifications = ReactiveCommand.CreateFromTask(OpenNotificationsExecuteAsync);
            OpenAbout = ReactiveCommand.CreateFromTask(OpenAboutExecuteAsync);
            Exit = ReactiveCommand.CreateFromTask(ExitExecuteAsync);

            Undo = ReactiveCommand.Create(UndoExecute);
            Redo = ReactiveCommand.Create(RedoExecute);

            Copy = ReactiveCommand.Create(() => _timelineParentViewModel.Copy());
            Paste = ReactiveCommand.Create(() => _timelineParentViewModel.Paste());
            Cut = ReactiveCommand.Create(() => _timelineParentViewModel.Cut());

            SettingsMenuItems = new ObservableCollection<object>();
            LoadSettingsMenuItems();

            RegisterCommands(keyBindingService);
        }

        private void LoadSettingsMenuItems()
        {
            SettingsMenuItems.Add(new MenuItem
            {
                Header = "環境設定",
                Command = OpenSettings,
            });

            var settingsProviders = _pluginService.GetSettingsProviders().ToList();
            if (settingsProviders.Count == 0)
            {
                return;
            }

            SettingsMenuItems.Add(new Separator());

            foreach (var settingsProvider in settingsProviders)
            {
                var openPluginSettingsCommand = ReactiveCommand.CreateFromTask(() =>
                    OpenPluginSettingsAsync(settingsProvider));
                SettingsMenuItems.Add(new MenuItem
                {
                    Header = settingsProvider.SettingsDisplayName,
                    Command = openPluginSettingsCommand,
                });
            }
        }

        private async Task OpenPluginSettingsAsync(IPluginSettingsProvider settingsProvider)
        {
            await OpenPluginSettingsInteraction.Handle(settingsProvider);
        }

        private void RegisterCommands(IKeyBindingService keyBindingService)
        {
            if (keyBindingService is not null)
            {
                keyBindingService.RegisterCommand("Undo", Undo);
                keyBindingService.RegisterCommand("Redo", Redo);
                keyBindingService.RegisterCommand("LoadEditingProject", LoadEditingProject);
                keyBindingService.RegisterCommand("CreateNewProject", CreateNewProject);
                keyBindingService.RegisterCommand("SaveEditingProject", SaveEditingProject);
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
                    MetasiaEditorProject editorProject = ProjectGenerator.CreateInMemoryProject(result.ProjectInfo, result.SelectedTemplate);
                    await _projectState.LoadProjectAsync(editorProject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"新規プロジェクト作成エラー: {ex.Message}");
                _notificationService.ShowError(
                    "新規プロジェクト作成失敗",
                    $"新しいプロジェクトを作成できませんでした。\n{ex.Message}");
            }
        }

        private async Task OpenSettingsExecuteAsync()
        {
            await OpenSettingsInteraction.Handle(Unit.Default);
        }

        private async Task SaveEditingProjectExecuteAsync()
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
                        _projectState.CurrentProject.ProjectPath = new DirectoryEntity(
                            Path.GetDirectoryName(targetFilePath)!);
                    }

                    _projectState.IsDirty = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロジェクト上書き保存エラー: {ex.Message}");
                _notificationService.ShowError(
                    "プロジェクト保存失敗",
                    $"プロジェクトの保存に失敗しました。\n{ex.Message}");
            }
        }

        private async Task LoadEditingProjectExecuteAsync()
        {
            try
            {
                var file = await _fileDialogService.OpenFileDialogAsync("プロジェクトを開く", new[] { "*.mtpj" });
                if (file is null) return;

                string filePath = file.Path.LocalPath;
                MetasiaEditorProject editorProject = ProjectSaveLoadManager.Load(filePath);
                editorProject.ProjectFilePath = filePath;
                await _projectState.LoadProjectAsync(editorProject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロジェクト読込エラー: {ex.Message}");
                _notificationService.ShowError(
                    "プロジェクト読込失敗",
                    $"プロジェクトを開けませんでした。\n{ex.Message}");
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
                _notificationService.ShowError(
                    "出力ウィンドウ表示失敗",
                    $"出力ウィンドウを開けませんでした。\n{ex.Message}");
            }
        }

        private async Task OpenPluginListExecuteAsync()
        {
            try
            {
                var vm = new PluginListViewModel(_pluginService);
                await PluginListInteraction.Handle(vm).FirstAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プラグイン一覧ウィンドウオープンエラー: {ex.Message}");
                _notificationService.ShowError(
                    "プラグイン一覧表示失敗",
                    $"プラグイン一覧を開けませんでした。\n{ex.Message}");
            }
        }

        private void OpenPluginFolderExecute()
        {
            try
            {
                MetasiaPaths.EnsureDirectoriesExist();
                Process.Start(new ProcessStartInfo
                {
                    FileName = MetasiaPaths.UserPluginsDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プラグインフォルダオープンエラー: {ex.Message}");
                _notificationService.ShowError(
                    "プラグインフォルダ表示失敗",
                    $"プラグインフォルダを開けませんでした。\n{ex.Message}");
            }
        }

        private async Task OpenNotificationsExecuteAsync()
        {
            try
            {
                await OpenNotificationsInteraction.Handle(_notificationCenterViewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"通知履歴ウィンドウオープンエラー: {ex.Message}");
                _notificationService.ShowError(
                    "通知履歴表示失敗",
                    $"通知履歴を開けませんでした。\n{ex.Message}");
            }
        }

        private async Task OpenAboutExecuteAsync()
        {
            try
            {
                await AboutInteraction.Handle(Unit.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"バージョン情報ウィンドウオープンエラー: {ex.Message}");
                _notificationService.ShowError(
                    "バージョン情報表示失敗",
                    $"バージョン情報を開けませんでした。\n{ex.Message}");
            }
        }

        private async Task ExitExecuteAsync()
        {
            await ExitInteraction.Handle(Unit.Default);
        }
    }
}
