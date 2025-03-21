using Metasia.Core.Objects;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Editor.Models.ProjectGenerate;

public class EmptyProjectTemplate : IProjectTemplate
{
    public string Name => "空のプロジェクト";
    public MetasiaProject Template { get; }

    public EmptyProjectTemplate(ProjectInfo projectInfo)
    {
        Template = new MetasiaProject(projectInfo);
        Template.LastFrame = projectInfo.Framerate * 5;

        // 基本レイヤーの作成
        LayerObject layer1 = new LayerObject("layer1", "Layer 1");

        // メインタイムラインの作成と設定
        TimelineObject mainTL = new TimelineObject("RootTimeline");
        mainTL.Layers.Add(layer1);

        // タイムラインをプロジェクトに追加
        Template.Timelines.Add(mainTL);
    }
} 