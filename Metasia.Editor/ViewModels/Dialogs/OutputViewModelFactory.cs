using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.PluginService;

namespace Metasia.Editor.ViewModels.Dialogs;

public class OutputViewModelFactory : IOutputViewModelFactory
{
    private readonly IProjectState _projectState;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPluginService _pluginService;

    public OutputViewModelFactory(
        IProjectState projectState,
        IFileDialogService fileDialogService,
        IPluginService pluginService)
    {
        _projectState = projectState;
        _fileDialogService = fileDialogService;
        _pluginService = pluginService;
    }
    public OutputViewModel Create()
    {
        return new OutputViewModel(_projectState, _fileDialogService, _pluginService);
    }
}