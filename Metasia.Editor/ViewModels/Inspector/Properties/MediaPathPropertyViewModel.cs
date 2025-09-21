using System.Windows.Input;
using Metasia.Core.Media;
using Metasia.Editor.Models.EditCommands;
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

    private MediaPath _target;
    private IEditCommandManager _editCommandManager;
    private IFileDialogService _fileDialogService;

    public MediaPathPropertyViewModel(
        string propertyIdentifier,
        MediaPath target,
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService
    )
    {
        _propertyDisplayName = propertyIdentifier;
        _target = target;
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _fileName = target.FileName;
        OpenFileCommand = ReactiveCommand.Create(OpenFileCommandExecute);
    }

    private void OpenFileCommandExecute()
    {
        var file = _fileDialogService.OpenFileDialogAsync("ファイルを開く", ["*.png", "*.jpg", "*.jpeg", "*.bmp"]);
    }
}
