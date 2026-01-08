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
        Template.LastFrame = projectInfo.Framerate * 10;

        TimelineObject mainTL = new("RootTimeline");
        for (int i = 1; i <= 100; i++)
        {
            var layer = new LayerObject($"layer{i}", $"Layer {i}");
            mainTL.Layers.Add(layer);
        }

        Template.Timelines.Add(mainTL);
    }
}