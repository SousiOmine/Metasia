using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands.Commands;
using NUnit.Framework;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands;

[TestFixture]
public class PasteClipsCommandTests
{
    [Test]
    public void Execute_PreservesOriginalLayerOffsets_WhenLowerLayerIsEmpty()
    {
        var timeline = new TimelineObject();
        var layer0 = new LayerObject("layer-0", "Layer 0");
        var layer1 = new LayerObject("layer-1", "Layer 1");
        timeline.Layers.Add(layer0);
        timeline.Layers.Add(layer1);

        var clipOnBaseLayer = new ClipObject("clip-0")
        {
            StartFrame = 0,
            EndFrame = 10
        };
        var clipOnUpperLayer = new ClipObject("clip-1")
        {
            StartFrame = 20,
            EndFrame = 30
        };

        var command = new PasteClipsCommand(
            timeline,
            new()
            {
                (clipOnBaseLayer, 0),
                (clipOnUpperLayer, 1)
            });

        command.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(command.PlacedClips[0].layer, Is.SameAs(layer0));
            Assert.That(command.PlacedClips[1].layer, Is.SameAs(layer1));
            Assert.That(layer0.Objects, Does.Contain(clipOnBaseLayer));
            Assert.That(layer1.Objects, Does.Contain(clipOnUpperLayer));
        });
    }
}
