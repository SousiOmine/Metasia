using Metasia.Editor.Models.States;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Dialogs;

public class OutputViewModelFactory : IOutputViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IFileDialogService _fileDialogService;

    public OutputViewModelFactory(
        IProjectState projectState,
        IFileDialogService fileDialogService)
    {
        _projectState = projectState;
        _fileDialogService = fileDialogService;
    }
    public OutputViewModel Create()
    {
        return new OutputViewModel(_projectState, _fileDialogService);
    }
}