using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.ViewModels;

public class NewObjectSelectViewModelFactory : INewObjectSelectViewModelFactory
{
    private readonly IPluginService _pluginService;

    public NewObjectSelectViewModelFactory(IPluginService pluginService)
    {
        ArgumentNullException.ThrowIfNull(pluginService);
        _pluginService = pluginService;
    }

    public NewObjectSelectViewModel Create(params NewObjectSelectViewModel.TargetType[] targetTypes)
    {
        return new NewObjectSelectViewModel(_pluginService, targetTypes);
    }
}
