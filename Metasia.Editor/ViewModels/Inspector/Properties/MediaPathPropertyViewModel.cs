using System;
using System.IO;
using System.Windows.Input;
using Metasia.Core.Media;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MediaPathPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    public ICommand OpenFileCommand { get; }

    private string _propertyDisplayName;
    private string _fileName;

    private readonly MediaPath _target;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IFileDialogService _fileDialogService;
    private readonly IProjectState _projectState;

    public MediaPathPropertyViewModel(
        string propertyIdentifier,
        MediaPath target,
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService,
        IProjectState projectState
    )
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        ArgumentNullException.ThrowIfNull(projectState);

        _propertyDisplayName = propertyIdentifier;
        _target = target;
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _projectState = projectState;
        _fileName = target?.FileName ?? "";
        OpenFileCommand = ReactiveCommand.Create(OpenFileCommandExecute);

        _projectState.TimelineChanged += TimelineChanged;
    }

    private void TimelineChanged()
    {
        _fileName = _target?.FileName ?? "";
    }

    private async void OpenFileCommandExecute()
    {
        var filePatterns = GetFilePatterns();
        var file = await _fileDialogService.OpenFileDialogAsync("ファイルを開く", filePatterns);

        if (file is null) return;

        var directory = Path.GetDirectoryName(file.Path?.LocalPath ?? "") ?? "";
        var fileName = Path.GetFileName(file.Path?.LocalPath ?? "");

        var mediaPath = MediaPath.CreateFromPath(directory, fileName);

        _editCommandManager.Execute(new MediaPathChangeCommand(_target, mediaPath));

    }

    private string[] GetFilePatterns()
    {
        // プロパティ表示名からオブジェクトタイプを判定
        if (_propertyDisplayName.Contains("Image") || _propertyDisplayName.Contains("画像"))
            return ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp", "*.tiff"];
        else if (_propertyDisplayName.Contains("Video") || _propertyDisplayName.Contains("動画"))
            return ["*.mp4", "*.avi", "*.mov", "*.wmv", "*.mkv", "*.webm", "*.flv"];
        else
            return ["*"]; // デフォルト
    }
}
