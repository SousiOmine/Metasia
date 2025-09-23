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

    public string FileName{
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    public ICommand OpenFileCommand { get; }
    
    private string _propertyDisplayName;
    private string _fileName;

    private readonly MediaPath _target;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IFileDialogService _fileDialogService;

    public MediaPathPropertyViewModel(
        string propertyIdentifier,
        MediaPath target,
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService
    )
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        
        _propertyDisplayName = propertyIdentifier;
        _target = target;
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _fileName = target?.FileName ?? "";
        OpenFileCommand = ReactiveCommand.Create(OpenFileCommandExecute);
    }

    private async void OpenFileCommandExecute()
    {
        var file = await _fileDialogService.OpenFileDialogAsync("ファイルを開く", ["*.png", "*.jpg", "*.jpeg", "*.bmp"]);

        if (file is null) return;

        var directory = Path.GetDirectoryName(file.Path?.LocalPath ?? "") ?? "";
        var fileName = Path.GetFileName(file.Path?.LocalPath ?? "");

        var mediaPath = MediaPath.CreateFromPath(directory, fileName, "", PathType.Absolute);

        _editCommandManager.Execute(new MediaPathChangeCommand(_target, mediaPath));
        
    }
}
