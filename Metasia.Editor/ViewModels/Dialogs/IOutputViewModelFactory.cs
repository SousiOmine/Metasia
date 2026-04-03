using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Dialogs;

public interface IOutputViewModelFactory
{
    OutputViewModel Create();
}