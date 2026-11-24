using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Collections.Generic;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;
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
        AudioChannelLayout audioChannelLayout = ResolveAudioChannelLayout(ProjectFile);
        ProjectInfo projectInfo = new ProjectInfo(ProjectFile.Framerate, new SKSize(ProjectFile.Resolution.Width, ProjectFile.Resolution.Height), ProjectFile.AudioSamplingRate, (int)audioChannelLayout);

        MetasiaProject project = new MetasiaProject(projectInfo);

        foreach (TimelineObject timeline in Timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
    }

    /// <summary>
    /// プロジェクトファイルの音声チャンネルレイアウトを決める。
    /// レイアウト名とチャンネル数が食い違う場合はチャンネル数を優先し、それも対応外ならステレオにする。
    /// </summary>
    private static AudioChannelLayout ResolveAudioChannelLayout(MetasiaProjectFile projectFile)
    {
        if (AudioChannelLayouts.TryParse(projectFile.AudioChannelLayout, out AudioChannelLayout namedLayout)
            && (int)namedLayout == projectFile.AudioChannels)
        {
            return namedLayout;
        }

        if (AudioChannelLayouts.TryFromChannelCount(projectFile.AudioChannels, out AudioChannelLayout layoutFromChannelCount))
        {
            return layoutFromChannelCount;
        }

        return AudioChannelLayout.Stereo;
    }
}

