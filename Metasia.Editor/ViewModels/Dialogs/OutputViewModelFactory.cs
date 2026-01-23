using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.PluginService;

namespace Metasia.Editor.ViewModels.Dialogs;

public class OutputViewModelFactory : IOutputViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPluginService _pluginService;
    private readonly IEncodeService _encodeService;
    private readonly MediaAccessorRouter _mediaAccessorRouter;

    public OutputViewModelFactory(
        IProjectState projectState,
        MediaAccessorRouter mediaAccessorRouter,
        IFileDialogService fileDialogService,
        IPluginService pluginService,
        IEncodeService encodeService)
    {
        _projectState = projectState;
        _mediaAccessorRouter = mediaAccessorRouter;
        _fileDialogService = fileDialogService;
        _pluginService = pluginService;
        _encodeService = encodeService;
    }
    public OutputViewModel Create()
    {
        return new OutputViewModel(_projectState, _mediaAccessorRouter, _fileDialogService, _pluginService, _encodeService);
    }
}