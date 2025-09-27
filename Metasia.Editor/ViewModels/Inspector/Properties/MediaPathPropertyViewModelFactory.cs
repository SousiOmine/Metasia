using System;
using Metasia.Core.Media;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MediaPathPropertyViewModelFactory : IMediaPathPropertyViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    private readonly IFileDialogService _fileDialogService;
    private readonly IProjectState _projectState;
    public MediaPathPropertyViewModelFactory(
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService,
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        ArgumentNullException.ThrowIfNull(projectState);
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _projectState = projectState;
    }

    public MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new MediaPathPropertyViewModel(propertyIdentifier, target, _editCommandManager, _fileDialogService, _projectState);
    }
}