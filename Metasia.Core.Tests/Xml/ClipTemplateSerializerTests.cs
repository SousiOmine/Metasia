using Metasia.Core.Objects;
using Metasia.Core.Objects.Templates;
using Metasia.Core.Xml;

namespace Metasia.Core.Tests.Xml
{
    [TestFixture]
    public class ClipTemplateSerializerTests
    {
        [Test]
        public void SerializeAndDeserialize_ShouldWorkCorrectly()
        {
            var videoClip = new VideoObject
            {
                Id = "test-clip-1",
                StartFrame = 0,
                EndFrame = 100
            };

            var timeline = new TimelineObject();
            timeline.Layers.Add(new LayerObject { Id = "layer-1" });
            timeline.Layers[0].Objects.Add(videoClip);

            var template = ClipTemplateSerializer.CreateFromClips(new[] { videoClip }, timeline);

            Assert.That(template, Is.Not.Null);
            Assert.That(template.ClipEntries, Has.Count.EqualTo(1));
            Assert.That(template.ClipEntries[0].LayerIndex, Is.EqualTo(0));
            Assert.That(template.ClipEntries[0].FrameOffset, Is.EqualTo(0));
            Assert.That(template.ClipEntries[0].ClipTypeName, Is.Not.Null.And.Not.Empty);

            var xml = ClipTemplateSerializer.Serialize(template);
            Assert.That(xml, Is.Not.Null.And.Not.Empty);
            Assert.That(xml, Does.Contain("ClipTemplate"));

            var deserialized = ClipTemplateSerializer.Deserialize(xml);
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.ClipEntries, Has.Count.EqualTo(1));
        }

        [Test]
        public void InstantiateClips_ShouldCreateClipsWithNewIds()
        {
            var videoClip = new VideoObject
            {
                Id = "original-id",
                StartFrame = 0,
                EndFrame = 100
            };

            var timeline = new TimelineObject();
            timeline.Layers.Add(new LayerObject { Id = "layer-1" });
            timeline.Layers[0].Objects.Add(videoClip);

            var template = ClipTemplateSerializer.CreateFromClips(new[] { videoClip }, timeline);

            var targetTimeline = new TimelineObject();
            targetTimeline.Layers.Add(new LayerObject { Id = "target-layer" });

            var clips = ClipTemplateSerializer.InstantiateClips(template, 50, 0, targetTimeline);

            Assert.That(clips, Has.Count.EqualTo(1));
            Assert.That(clips[0].clip.Id, Is.Not.EqualTo("original-id"));
            Assert.That(clips[0].clip.StartFrame, Is.EqualTo(50));
            Assert.That(clips[0].clip.EndFrame, Is.EqualTo(150));
            Assert.That(clips[0].layerIndex, Is.EqualTo(0));
        }

        [Test]
        public void InstantiateClips_WithMultipleLayers_ShouldWork()
        {
            var clip1 = new VideoObject { Id = "clip-1", StartFrame = 0, EndFrame = 100 };
            var clip2 = new VideoObject { Id = "clip-2", StartFrame = 50, EndFrame = 150 };

            var timeline = new TimelineObject();
            timeline.Layers.Add(new LayerObject { Id = "layer-0" });
            timeline.Layers.Add(new LayerObject { Id = "layer-1" });
            timeline.Layers[0].Objects.Add(clip1);
            timeline.Layers[1].Objects.Add(clip2);

            var template = ClipTemplateSerializer.CreateFromClips(new[] { clip1, clip2 }, timeline);

            Assert.That(template.ClipEntries, Has.Count.EqualTo(2));

            var targetTimeline = new TimelineObject();
            targetTimeline.Layers.Add(new LayerObject { Id = "target-layer-0" });
            targetTimeline.Layers.Add(new LayerObject { Id = "target-layer-1" });

            var clips = ClipTemplateSerializer.InstantiateClips(template, 100, 0, targetTimeline);

            Assert.That(clips, Has.Count.EqualTo(2));

            Assert.Multiple(() =>
            {
                Assert.That(clips[0].clip.StartFrame, Is.EqualTo(100));
                Assert.That(clips[0].clip.EndFrame, Is.EqualTo(200));
                Assert.That(clips[0].layerIndex, Is.EqualTo(0));
            });
            
            Assert.Multiple(() =>
            {
                Assert.That(clips[1].clip.StartFrame, Is.EqualTo(150));
                Assert.That(clips[1].clip.EndFrame, Is.EqualTo(250));
                Assert.That(clips[1].layerIndex, Is.EqualTo(1));
            });
        }
    }
}