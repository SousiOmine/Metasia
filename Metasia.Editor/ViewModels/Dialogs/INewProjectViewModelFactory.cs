using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Dialogs;

public interface INewProjectViewModelFactory
{
    NewProjectViewModel Create();
}