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
        set
        {
            this.RaiseAndSetIfChanged(ref _targetPlayerViewModel, value);
            if (value is not null)
            {
                TargetTimelineName = value.TargetTimeline.Id;
            }
        } 
    }

    public string TargetTimelineName
    {
        get => _targetTimelineName;
        set => this.RaiseAndSetIfChanged(ref _targetTimelineName, value);
    }

    private MetasiaProject? currentProject;
    private PlayerViewModel? _targetPlayerViewModel;
    private string _targetTimelineName;
    
    
    public PlayerParentViewModel(MetasiaProject project)
    {
        CurrentProject = project;
    }

    private void LoadProject()
    {
        TargetPlayerViewModel = new PlayerViewModel(CurrentProject.Timelines[0], CurrentProject.Info);
        
    }
}