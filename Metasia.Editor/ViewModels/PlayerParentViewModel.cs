using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using ReactiveUI;
using System.Linq;
using Metasia.Editor.Services;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase, IDisposable
{

    public PlayerViewModel? TargetPlayerViewModel
    {
        get => _targetPlayerViewModel;
        set
        {
            if (ReferenceEquals(_targetPlayerViewModel, value))
            {
                return;
            }

            // 以前のコマンド登録を解除
            UnregisterPlayerCommands();
            var oldPlayerViewModel = _targetPlayerViewModel;

            this.RaiseAndSetIfChanged(ref _targetPlayerViewModel, value);
            oldPlayerViewModel?.Dispose();

            if (value is not null)
            {
                TargetTimelineName = value.TargetTimeline.Id;
                // 新しいPlayerViewModelのコマンドを登録
                RegisterPlayerCommands(value);
            }
            else
            {
                TargetTimelineName = string.Empty;
            }
        }
    }

    public string TargetTimelineName
    {
        get => _targetTimelineName;
        set => this.RaiseAndSetIfChanged(ref _targetTimelineName, value);
    }

    public bool IsPlayerShow
    {
        get => _isPlayerShow;
        set => this.RaiseAndSetIfChanged(ref _isPlayerShow, value);
    }

    private PlayerViewModel? _targetPlayerViewModel;
    private string _targetTimelineName = string.Empty;

    private bool _isPlayerShow = false;

    private readonly IKeyBindingService? _keyBindingService;
    private readonly IPlayerViewModelFactory _playerViewModelFactory;
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly ISelectionState _selectionState;
    public PlayerParentViewModel(
        IKeyBindingService keyBindingService,
        IPlayerViewModelFactory playerViewModelFactory,
        IProjectState projectState,
        IEditCommandManager editCommandManager,
        ISelectionState selectionState)
    {
        // キーバインディングサービスを設定
        if (keyBindingService is not null)
        {
            _keyBindingService = keyBindingService;
            SetKeyBindingService(keyBindingService);
        }
        _playerViewModelFactory = playerViewModelFactory;
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _selectionState = selectionState;
        _projectState.ProjectLoaded += OnProjectLoaded;
        _projectState.ProjectClosed += OnProjectClosed;
        _editCommandManager.CommandExecuted += ValidateCurrentTimeline;
        _editCommandManager.CommandUndone += ValidateCurrentTimeline;
        _editCommandManager.CommandRedone += ValidateCurrentTimeline;
    }

    /// <summary>
    /// ProjectLoadedイベントハンドラー
    /// </summary>
    private void OnProjectLoaded()
    {
        LoadProject();
    }

    /// <summary>
    /// PlayerViewModelのコマンドをキーバインディングサービスに登録
    /// </summary>
    private void RegisterPlayerCommands(PlayerViewModel playerViewModel)
    {
        RegisterCommand("PlayPauseToggle", playerViewModel.PlayPauseToggle);
    }

    /// <summary>
    /// 以前のPlayerViewModelのコマンド登録を解除
    /// </summary>
    private void UnregisterPlayerCommands()
    {
        // 登録済みのコマンドを解除
        _keyBindingService?.UnregisterCommand("PlayPauseToggle");
    }

    // public void LoadProject(MetasiaEditorProject editorProject)
    // {
    //     _projectState.LoadProjectAsync(editorProject);

    // }


    public bool TryUndo()
    {
        if (_editCommandManager.CanUndo)
        {
            _editCommandManager.Undo();
            return true;
        }
        return false;
    }

    public bool TryRedo()
    {
        if (_editCommandManager.CanRedo)
        {
            _editCommandManager.Redo();
            return true;
        }
        return false;
    }

    private void LoadProject()
    {
        IsPlayerShow = false;

        _editCommandManager.Clear();
        TargetPlayerViewModel = null;

        if (_projectState.CurrentProjectInfo is null || _projectState.CurrentProject is null)
        {
            return;
        }

        var initialTimeline = _projectState.CurrentTimeline ?? _projectState.CurrentProject.Timelines.FirstOrDefault();
        if (initialTimeline is null)
        {
            return;
        }

        _projectState.SetCurrentTimeline(initialTimeline);
        TargetPlayerViewModel = CreatePlayerViewModel(initialTimeline);
        IsPlayerShow = true;
    }

    public void SwitchToTimeline(TimelineObject timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        if (_projectState.CurrentProjectInfo is null || _projectState.CurrentProject is null)
        {
            return;
        }

        if (_projectState.CurrentTimeline?.Id == timeline.Id && TargetPlayerViewModel?.TargetTimeline.Id == timeline.Id)
        {
            return;
        }

        TargetPlayerViewModel?.PauseAndSeekToFrame(0);
        _selectionState.ClearSelectedClips();
        _selectionState.ClearSelectedLayer();
        _projectState.SetCurrentTimeline(timeline);

        TargetPlayerViewModel = CreatePlayerViewModel(timeline);
        IsPlayerShow = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projectState.ProjectLoaded -= OnProjectLoaded;
            _projectState.ProjectClosed -= OnProjectClosed;
            _editCommandManager.CommandExecuted -= ValidateCurrentTimeline;
            _editCommandManager.CommandUndone -= ValidateCurrentTimeline;
            _editCommandManager.CommandRedone -= ValidateCurrentTimeline;
            UnregisterPlayerCommands();
            TargetPlayerViewModel = null;
        }

        base.Dispose(disposing);
    }

    private void OnProjectClosed()
    {
        TargetPlayerViewModel = null;
        IsPlayerShow = false;
        TargetTimelineName = string.Empty;
    }

    private PlayerViewModel CreatePlayerViewModel(TimelineObject timeline)
    {
        return _playerViewModelFactory.Create(timeline, _projectState.CurrentProjectInfo!);
    }

    private void ValidateCurrentTimeline(object? sender, IEditCommand e)
    {
        if (_projectState.CurrentProject is null) return;
        if (TargetPlayerViewModel is null) return;

        var timelineExists = _projectState.CurrentProject.Timelines
            .Any(t => t.Id == TargetPlayerViewModel.TargetTimeline.Id);

        if (!timelineExists)
        {
            SwitchToRootTimeline();
        }
    }

    private void SwitchToRootTimeline()
    {
        if (_projectState.CurrentProject is null) return;

        var rootTimelineId = _projectState.CurrentProject.ProjectFile.RootTimelineId;
        var rootTimeline = _projectState.CurrentProject.Timelines
            .FirstOrDefault(t => t.Id == rootTimelineId)
            ?? _projectState.CurrentProject.Timelines.FirstOrDefault();

        if (rootTimeline is not null &&
            rootTimeline.Id != TargetPlayerViewModel?.TargetTimeline.Id)
        {
            SwitchToTimeline(rootTimeline);
        }
    }
}
