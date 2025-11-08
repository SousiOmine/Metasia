using System;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Dialogs;

public class NewProjectViewModelFactory : INewProjectViewModelFactory
{
    private readonly IFileDialogService _fileDialogService;

    public NewProjectViewModelFactory(IFileDialogService fileDialogService)
    {
        ArgumentNullException.ThrowIfNull(fileDialogService);
        _fileDialogService = fileDialogService;
    }

    public NewProjectViewModel Create()
    {
        return new NewProjectViewModel(_fileDialogService);
    }
}