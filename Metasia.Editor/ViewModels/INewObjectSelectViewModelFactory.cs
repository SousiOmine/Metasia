using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.ViewModels;

public interface INewObjectSelectViewModelFactory
{
    NewObjectSelectViewModel Create(params NewObjectSelectViewModel.TargetType[] targetTypes);
}
