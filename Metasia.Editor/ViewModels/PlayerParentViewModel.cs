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

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase
{
    public MetasiaProject? CurrentProject
    {
        get { return currentProject;}
        set
        {

            currentProject = value;
            if (value is not null)
            {
                // 新しいProjectがセットされたら、メインタイムラインのPlayerViewModelを作成
                var mainTimeline = value.Timelines.FirstOrDefault();
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
                        var newVM = new PlayerViewModel(mainTimeline, value.Info);
                        _playerViewModels.Add(newVM);
                        TargetPlayerViewModel = newVM;
                    }
                }
            }
            ProjectInstanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public MetasiaEditorProject? CurrentEditorProject { get; set; }

    public event EventHandler? ProjectInstanceChanged;

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

    private MetasiaProject? currentProject;
    private PlayerViewModel? _targetPlayerViewModel;
    private string _targetTimelineName = string.Empty;
    private MetasiaEditorProject? currentEditorProject;

    private List<PlayerViewModel> _playerViewModels = new();

    private bool _isPlayerShow = false;

    public PlayerParentViewModel()
    {
        // キーバインディングサービスを設定
        var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        if (keyBindingService is not null)
        {
            SetKeyBindingService(keyBindingService);
        }
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
        var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        keyBindingService?.UnregisterCommand("PlayPauseToggle");
    }

    public void LoadProject(MetasiaEditorProject editorProject)
    {
        IsPlayerShow = false;
        CurrentEditorProject = editorProject;

        // 既存のPlayerViewModelリストをクリア
        _playerViewModels.Clear();

        ProjectInfo projectInfo = new ProjectInfo()
        {
            Framerate = editorProject.ProjectFile.Framerate,
            Size = new SKSize(editorProject.ProjectFile.Resolution.Width, editorProject.ProjectFile.Resolution.Height),
        };

        // タイムラインごとに新しいPlayerViewModelを作成
        foreach (TimelineFile timeline in editorProject.Timelines)
        {
            _playerViewModels.Add(new PlayerViewModel(timeline.Timeline, projectInfo));
        }

        // CurrentProjectをセット (setterでPlayerViewModelも設定される)
        CurrentProject = editorProject.CreateMetasiaProject();

        IsPlayerShow = true;
    }

    public bool TryUndo()
    {
        if (TargetPlayerViewModel is null) return false;
        if (TargetPlayerViewModel.CanUndo)
        {
            TargetPlayerViewModel.Undo();
            return true;
        }
        return false;
    }
    
    public bool TryRedo()
    {
        if (TargetPlayerViewModel is null) return false;
        if (TargetPlayerViewModel.CanRedo)
        {
            TargetPlayerViewModel.Redo();
            return true;
        }
        return false;
    }
}