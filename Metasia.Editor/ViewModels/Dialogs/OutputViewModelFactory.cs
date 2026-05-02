using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Services.PluginService;
using Metasia.Core.Render;

namespace Metasia.Editor.ViewModels.Dialogs;

public class OutputViewModelFactory : IOutputViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPluginService _pluginService;
    private readonly IEncodeService _encodeService;
    private readonly INotificationService _notificationService;
    private readonly MediaAccessorRouter _mediaAccessorRouter;
    private readonly IRenderSurfaceFactory _renderSurfaceFactory;

    public OutputViewModelFactory(
        IProjectState projectState,
        MediaAccessorRouter mediaAccessorRouter,
        IFileDialogService fileDialogService,
        IPluginService pluginService,
        IEncodeService encodeService,
        INotificationService notificationService,
        IRenderSurfaceFactory renderSurfaceFactory)
    {
        _projectState = projectState;
        _mediaAccessorRouter = mediaAccessorRouter;
        _fileDialogService = fileDialogService;
        _pluginService = pluginService;
        _encodeService = encodeService;
        _notificationService = notificationService;
        _renderSurfaceFactory = renderSurfaceFactory;
    }
    public OutputViewModel Create()
    {
        return new OutputViewModel(_projectState, _mediaAccessorRouter, _fileDialogService, _pluginService, _encodeService, _notificationService, _renderSurfaceFactory);
    }
}
