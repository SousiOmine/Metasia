using System;
using System.Collections.Generic;
using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;
using ReactiveUI;
using SkiaSharp;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using System.Windows.Input;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase, IDisposable
{

    public PlayerViewModel? TargetPlayerViewModel
    {
        get => _targetPlayerViewModel;
        set
        {
            // 以前のコマンド登録を解除
            UnregisterPlayerCommands();

            this.RaiseAndSetIfChanged(ref _targetPlayerViewModel, value);
            if (value is not null)
            {
                TargetTimelineName = value.TargetTimeline.Id;
                // 新しいPlayerViewModelのコマンドを登録
                RegisterPlayerCommands(value);
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

    private List<PlayerViewModel> _playerViewModels = new();

    private bool _isPlayerShow = false;

    private readonly IKeyBindingService? _keyBindingService;
    private readonly IPlayerViewModelFactory _playerViewModelFactory;
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    public PlayerParentViewModel(IKeyBindingService keyBindingService, IPlayerViewModelFactory playerViewModelFactory, IProjectState projectState, IEditCommandManager editCommandManager)
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
        _projectState.ProjectLoaded += OnProjectLoaded;
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

        // 既存のPlayerViewModelリストをクリア
        _playerViewModels.Clear();
        _editCommandManager.Clear();

        if (_projectState.CurrentProjectInfo is null || _projectState.CurrentProject is null)
        {
            return;
        }

        ProjectInfo projectInfo = new ProjectInfo(_projectState.CurrentProjectInfo.Framerate, new SKSize(_projectState.CurrentProjectInfo.Size.Width, _projectState.CurrentProjectInfo.Size.Height), _projectState.CurrentProjectInfo.AudioSamplingRate, _projectState.CurrentProjectInfo.AudioChannels);

        // タイムラインごとに新しいPlayerViewModelを作成
        foreach (TimelineFile timeline in _projectState.CurrentProject.Timelines)
        {
            _playerViewModels.Add(_playerViewModelFactory.Create(timeline.Timeline, projectInfo));
        }

        IsPlayerShow = true;

        if (_projectState.CurrentProject is not null)
        {
            // 新しいProjectがセットされたら、メインタイムラインのPlayerViewModelを作成
            var mainTimeline = _projectState.CurrentProject.Timelines.FirstOrDefault()?.Timeline;
            if (mainTimeline != null)
            {
                // 既存のPlayerViewModelsを確認
                var existingVM = _playerViewModels.FirstOrDefault(vm => vm.TargetTimeline.Id == mainTimeline.Id);

                if (existingVM != null)
                {
                    // 既存のVMがあればそれを使用
                    TargetPlayerViewModel = existingVM;
                }
                else
                {
                    // なければ新しく作成
                    var newVM = _playerViewModelFactory.Create(mainTimeline, _projectState.CurrentProjectInfo);
                    _playerViewModels.Add(newVM);
                    TargetPlayerViewModel = newVM;
                }
            }
        }
    }

    /// <summary>
    /// リソースを解放します
    /// </summary>
    public new void Dispose()
    {
        _projectState.ProjectLoaded -= OnProjectLoaded;
        UnregisterPlayerCommands();
        base.Dispose();
    }
}