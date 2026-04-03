using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Core.Media;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Services;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MediaPathPropertyViewModelFactory : IMediaPathPropertyViewModelFactory
{
    private readonly IEditCommandManager _editCommandManager;
    private readonly IFileDialogService _fileDialogService;
    private readonly IProjectState _projectState;
    private readonly ISettingsService _settingsService;

    public MediaPathPropertyViewModelFactory(
        IEditCommandManager editCommandManager,
        IFileDialogService fileDialogService,
        IProjectState projectState,
        ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(settingsService);
        _editCommandManager = editCommandManager;
        _fileDialogService = fileDialogService;
        _projectState = projectState;
        _settingsService = settingsService;
    }

    public MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new MediaPathPropertyViewModel(propertyIdentifier, target, _editCommandManager, _fileDialogService, _projectState, _settingsService);
    }
}