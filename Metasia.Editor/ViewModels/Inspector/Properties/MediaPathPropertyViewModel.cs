using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

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
    private readonly ISettingsService _settingsService;
    private readonly bool _allowMultiClipApply;
    private readonly IMetasiaObject? _owner;

    public MediaPathPropertyViewModel(
        string propertyIdentifier,
        MediaPath target,
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService,
        IProjectState projectState,
        ISettingsService settingsService,
        bool allowMultiClipApply = true,
        IMetasiaObject? owner = null
    )
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(settingsService);

        _propertyDisplayName = propertyIdentifier;
        _target = target;
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _projectState = projectState;
        _settingsService = settingsService;
        _allowMultiClipApply = allowMultiClipApply;
        _owner = owner;
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

        bool saveAsRelative = _settingsService.CurrentSettings.General.MediaPathStyle == MediaPathStyle.Relative;
        if (saveAsRelative && _projectState.CurrentProject?.ProjectFilePath == null)
        {
            var saved = await ProjectSaveHelper.EnsureProjectSavedAsync(_projectState, _fileDialogService);
            if (!saved) return;
        }

        var projectDir = _projectState.CurrentProject?.ProjectPath.Path;
        var mediaPath = MediaPath.CreateFromPath(directory, fileName, projectDir, saveAsRelative);

        _editCommandManager.Execute(new MediaPathChangeCommand(_target, mediaPath));
    }

    private string[] GetFilePatterns()
    {
        var patterns = new List<string>();

        if (_target.Types is null)
        {
            return ["*"];
        }

        foreach (var type in _target.Types)
        {
            switch (type)
            {
                case MediaType.Image:
                    patterns.AddRange(["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp", "*.tiff"]);
                    break;
                case MediaType.Video:
                    patterns.AddRange(["*.mp4", "*.avi", "*.mov", "*.wmv", "*.mkv", "*.webm", "*.flv"]);
                    break;
                case MediaType.Audio:
                    patterns.AddRange(["*.mp3", "*.wav", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma"]);
                    break;
                default:
                    // ここまでで特に設定されていないやつであれば全拡張子を受け入れる
                    patterns.Add("*");
                    break;
            }
        }

        return patterns.Count > 0 ? patterns.ToArray() : ["*"];
    }
}