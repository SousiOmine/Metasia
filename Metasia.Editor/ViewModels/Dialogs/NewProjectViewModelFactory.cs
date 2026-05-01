namespace Metasia.Editor.ViewModels.Dialogs;

public class NewProjectViewModelFactory : INewProjectViewModelFactory
{
    public NewProjectViewModel Create()
    {
        return new NewProjectViewModel();
    }
}