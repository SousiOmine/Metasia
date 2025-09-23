using System;
using Metasia.Core.Media;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MediaPathPropertyViewModelFactory : IMediaPathPropertyViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    private readonly IFileDialogService _fileDialogService;

    public MediaPathPropertyViewModelFactory(
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
    }

    public MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new MediaPathPropertyViewModel(propertyIdentifier, target, _editCommandManager, _fileDialogService);
    }
}