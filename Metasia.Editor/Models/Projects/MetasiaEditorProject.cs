using System.Collections.Generic;
using Metasia.Core.Project;
using Metasia.Editor.Models.FileSystem;

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
}

