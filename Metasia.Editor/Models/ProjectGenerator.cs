using System.IO;
using System.Text.Json;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Models;

public class ProjectGenerator
{
    public static MetasiaEditorProject CreateProject(string projectPath, ProjectInfo projectInfo, MetasiaProject? templateProject = null)
    {
        MetasiaProject project;
        if (templateProject != null)
        {
            project = templateProject;
        }
        else
        {
            //空の新しいプロジェクトを作成
            project = new MetasiaProject(projectInfo);
            project.LastFrame = projectInfo.Framerate * 5;

            LayerObject layer1 = new LayerObject("layer1", "Layer 1");

            TimelineObject mainTL = new TimelineObject("RootTimeline");
            mainTL.Layers.Add(layer1);
            project.Timelines.Add(mainTL);
        }

        //プロジェクトフォルダが存在しない場合は作成
        if (!Directory.Exists(projectPath))
        {
            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, "Timelines"));
        }
        else if (!Directory.Exists(Path.Combine(projectPath, "Timelines")))
        {
            // Timelinesフォルダが確実に存在するようにする
            Directory.CreateDirectory(Path.Combine(projectPath, "Timelines"));
        }

        MetasiaProjectFile projectFile = new MetasiaProjectFile()
        {
            Framerate = projectInfo.Framerate,
            Resolution = new VideoResolution() { Width = projectInfo.Size.Width, Height = projectInfo.Size.Height },
        };

        MetasiaEditorProject editorProject = new MetasiaEditorProject(new DirectoryEntity(projectPath), projectFile);

        foreach (var timeline in project.Timelines)
        {
            TimelineFile timelineFile = new TimelineFile(new FileEntity(Path.Combine(projectPath,"Timelines", $"{timeline.Id}.mttl")), timeline);
            editorProject.Timelines.Add(timelineFile);
        }


        ProjectSaveLoadManager.Save(editorProject);

        return editorProject;
    }
    
}
