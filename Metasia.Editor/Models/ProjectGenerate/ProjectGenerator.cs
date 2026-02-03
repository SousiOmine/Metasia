using System;
using System.IO;
using System.Text.Json;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Models.ProjectGenerate;

public class ProjectGenerator
{
    public static MetasiaEditorProject CreateProject(string projectFilePath, ProjectInfo projectInfo, MetasiaProject? templateProject = null)
    {
        MetasiaProject project;
        if (templateProject is not null)
        {
            project = templateProject;
        }
        else
        {
            // 空のプロジェクトテンプレートを使用
            project = new EmptyProjectTemplate(projectInfo).Template;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        // プロジェクトファイルの親ディレクトリが存在しない場合は作成
        string? parentDirectory = Path.GetDirectoryName(projectFilePath);
        if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        string fullProjectPath = Path.GetFullPath(projectFilePath);
        string? resolvedDirectory = Path.GetDirectoryName(fullProjectPath);
        if (string.IsNullOrWhiteSpace(resolvedDirectory))
        {
            resolvedDirectory = Directory.GetCurrentDirectory();
        }

        MetasiaProjectFile projectFile = new()
        {
            Framerate = projectInfo.Framerate,
            Resolution = new VideoResolution() { Width = projectInfo.Size.Width, Height = projectInfo.Size.Height },
        };

        DirectoryEntity projectDirectory = new(resolvedDirectory);
        MetasiaEditorProject editorProject = new(projectDirectory, projectFile)
        {
            ProjectFilePath = projectFilePath
        };

        foreach (var timeline in project.Timelines)
        {
            editorProject.Timelines.Add(timeline);
        }

        // 単一ファイルとして保存
        ProjectSaveLoadManager.Save(editorProject, projectFilePath);

        return editorProject;
    }
}
