using System.Collections.Generic;
using Metasia.Core.Project;
using Metasia.Editor.Models.FileSystem;
using SkiaSharp;

namespace Metasia.Editor.Models.Projects;

/// <summary>
/// プロジェクトを構成する各種ファイルとプロジェクトを紐づけて管理するクラス
/// </summary>
public class MetasiaEditorProject
{
    public DirectoryEntity ProjectPath { get; set; }

    public MetasiaProjectFile ProjectFile { get; set; }

    public List<TimelineFile> Timelines { get; set; } = new();

    public MetasiaEditorProject(DirectoryEntity projectPath, MetasiaProjectFile projectFile)
    {
        ProjectPath = projectPath;
        ProjectFile = projectFile;
    }

    public MetasiaProject CreateMetasiaProject()
    {
        ProjectInfo projectInfo = new ProjectInfo()
        {
            Framerate = ProjectFile.Framerate,
            Size = new SKSize(ProjectFile.Resolution.Width, ProjectFile.Resolution.Height),
        };

        MetasiaProject project = new MetasiaProject(projectInfo);

        foreach (TimelineFile timeline in Timelines)
        {
            project.Timelines.Add(timeline.Timeline);
        }

        return project;
    }
}

