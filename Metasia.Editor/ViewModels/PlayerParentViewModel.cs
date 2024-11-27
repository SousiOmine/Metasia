using System;
using Metasia.Core.Project;
using ReactiveUI;

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase
{
    public MetasiaProject? CurrentProject
    {
        get { return currentProject;}
        set
        {
            currentProject = value;
            if(value is not null) LoadProject();
        }
    }

    public PlayerViewModel TargetPlayerViewModel
    {
        get => _targetPlayerViewModel;
        set => this.RaiseAndSetIfChanged(ref _targetPlayerViewModel, value);
    }

    private MetasiaProject? currentProject;
    private PlayerViewModel? _targetPlayerViewModel;
    
    public PlayerParentViewModel()
    {
        //TargetPlayerViewModel = new PlayerViewModel(null);
    }
    
    public PlayerParentViewModel(MetasiaProject project)
    {
        CurrentProject = project;
    }

    private void LoadProject()
    {
        TargetPlayerViewModel = new PlayerViewModel(CurrentProject.Timelines[0], CurrentProject.Info);
        
    }
}