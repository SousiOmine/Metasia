using System.Collections.Generic;
using Metasia.Core.Objects;
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

    /// <summary>
    /// プロジェクトファイルのパス（.mtpjファイル）
    /// </summary>
    public string? ProjectFilePath { get; set; }

    public MetasiaProjectFile ProjectFile { get; set; }

    public List<TimelineObject> Timelines { get; set; } = new();

    public MetasiaEditorProject(DirectoryEntity projectPath, MetasiaProjectFile projectFile)
    {
        ProjectPath = projectPath;
        ProjectFile = projectFile;
    }

    public MetasiaProject CreateMetasiaProject()
    {
        ProjectInfo projectInfo = new ProjectInfo(ProjectFile.Framerate, new SKSize(ProjectFile.Resolution.Width, ProjectFile.Resolution.Height), ProjectFile.AudioSamplingRate, ProjectFile.AudioChannels);

        MetasiaProject project = new MetasiaProject(projectInfo);

        foreach (TimelineObject timeline in Timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
    }
}

