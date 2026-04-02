using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.ViewModels;

public interface INewObjectSelectViewModelFactory
{
    NewObjectSelectViewModel Create(params NewObjectSelectViewModel.TargetType[] targetTypes);
}
