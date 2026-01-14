using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs;

/// <summary>
/// 動画出力ウィンドウのViewModel
/// </summary>
public class OutputViewModel : ViewModelBase
{

    public ObservableCollection<string> OutputMethodList { get; } = new ObservableCollection<string>();

    public ObservableCollection<string> TimelineList { get; } = new ObservableCollection<string>();

    public string OutputPath
    {
        get => _outputPath;
        set => this.RaiseAndSetIfChanged(ref _outputPath, value);
    }

    public ICommand SelectOutputPathCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CancelAction { get; set; }


    private string _outputPath = string.Empty;
    private readonly IProjectState _projectState;
    private readonly IFileDialogService _fileDialogService;

    public OutputViewModel(
        IProjectState projectState,
        IFileDialogService fileDialogService
    )
    {
        _projectState = projectState;
        _fileDialogService = fileDialogService;

        CancelCommand = ReactiveCommand.Create(() => Cancel());
        SelectOutputPathCommand = ReactiveCommand.CreateFromTask(SelectOutputPathExecuteAsync);

        OutputMethodList.Add("連番画像出力");
        OutputMethodList.Add("FFmpegPlugin");

        _projectState.ProjectLoaded += UIReflesh;
        _projectState.ProjectClosed += UIReflesh;
        _projectState.TimelineChanged += UIReflesh;

        UIReflesh();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projectState.ProjectLoaded -= UIReflesh;
            _projectState.ProjectClosed -= UIReflesh;
            _projectState.TimelineChanged -= UIReflesh;
        }

        base.Dispose(disposing);
    }

    private void UIReflesh()
    {
        TimelineList.Clear();
        if (_projectState.CurrentProject is null) return;

        foreach (var timeline in _projectState.CurrentProject.Timelines)
        {
            TimelineList.Add(timeline.Timeline.Id);
        }
    }

    private async Task SelectOutputPathExecuteAsync()
    {
        var result = await _fileDialogService.SaveFileDialogAsync("出力先を選択", new string[] { "*.avi", "*.mp4", "*.mov", "*.mkv", "*.webm", "*.gif" });
        if (result is null) return;

        OutputPath = result.Path?.LocalPath ?? "";
    }

    private void Cancel()
    {
        CancelAction?.Invoke();
    }
}

