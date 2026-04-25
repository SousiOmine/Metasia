using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects
{
    [TestFixture]
    public class TimelineObjectTests
    {
        private TimelineObject _timelineObject = null!;

        [SetUp]
        public void Setup()
        {
            _timelineObject = new TimelineObject("timeline-id");
        }

        [Test]
        public void Constructor_WithId_InitializesCorrectly()
        {
            var timeline = new TimelineObject("test-id");

            Assert.That(timeline.Id, Is.EqualTo("test-id"));
            Assert.That(timeline.Volume.Value, Is.EqualTo(100.0));
            Assert.That(timeline.Layers, Is.Not.Null);
            Assert.That(timeline.Layers, Is.InstanceOf<List<LayerObject>>());
            Assert.That(timeline.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithoutParameters_InitializesWithDefaults()
        {
            var timeline = new TimelineObject();

            Assert.That(timeline.Volume.Value, Is.EqualTo(100.0));
            Assert.That(timeline.Layers, Is.Not.Null);
            Assert.That(timeline.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void Volume_CanBeModified()
        {
            Assert.That(_timelineObject.Volume.Value, Is.EqualTo(100.0));

            _timelineObject.Volume = 75;

            Assert.That(_timelineObject.Volume.Value, Is.EqualTo(75.0));
        }

        [Test]
        public void Layers_CanAddAndRemove()
        {
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");

            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);

            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(2));
            Assert.That(_timelineObject.Layers[0].Id, Is.EqualTo("layer1"));
            Assert.That(_timelineObject.Layers[1].Id, Is.EqualTo("layer2"));

            _timelineObject.Layers.Remove(layer1);

            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(1));
            Assert.That(_timelineObject.Layers[0].Id, Is.EqualTo("layer2"));
        }

        [Test]
        public void Layers_MaintainOrder()
        {
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            var layer3 = new LayerObject("layer3", "Layer 3");

            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer2);
            _timelineObject.Layers.Add(layer3);

            Assert.That(_timelineObject.Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(_timelineObject.Layers[1].Name, Is.EqualTo("Layer 2"));
            Assert.That(_timelineObject.Layers[2].Name, Is.EqualTo("Layer 3"));
        }

        [Test]
        public void Layers_CanInsertAtSpecificIndex()
        {
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");
            var layer3 = new LayerObject("layer3", "Layer 3");

            _timelineObject.Layers.Add(layer1);
            _timelineObject.Layers.Add(layer3);

            _timelineObject.Layers.Insert(1, layer2);

            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(3));
            Assert.That(_timelineObject.Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(_timelineObject.Layers[1].Name, Is.EqualTo("Layer 2"));
            Assert.That(_timelineObject.Layers[2].Name, Is.EqualTo("Layer 3"));
        }

        [Test]
        public void Layers_CanClear()
        {
            _timelineObject.Layers.Add(new LayerObject("layer1", "Layer 1"));
            _timelineObject.Layers.Add(new LayerObject("layer2", "Layer 2"));
            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(2));

            _timelineObject.Layers.Clear();

            Assert.That(_timelineObject.Layers.Count, Is.EqualTo(0));
        }

        [Test]
        public void IMetasiaObject_Properties_WorkCorrectly()
        {
            Assert.That(_timelineObject.Id, Is.EqualTo("timeline-id"));
            Assert.That(_timelineObject.IsActive, Is.True);

            _timelineObject.Id = "modified-id";
            _timelineObject.IsActive = false;

            Assert.That(_timelineObject.Id, Is.EqualTo("modified-id"));
            Assert.That(_timelineObject.IsActive, Is.False);
        }

        [Test]
        public void SelectionRange_DefaultValues()
        {
            var timeline = new TimelineObject();

            Assert.That(timeline.SelectionStart, Is.EqualTo(0));
            Assert.That(timeline.SelectionEnd, Is.EqualTo(600));
        }

        [Test]
        public void SelectionRange_CanBeModified()
        {
            var timeline = new TimelineObject();

            timeline.SelectionStart = 50;
            timeline.SelectionEnd = 200;

            Assert.That(timeline.SelectionStart, Is.EqualTo(50));
            Assert.That(timeline.SelectionEnd, Is.EqualTo(200));
        }

        [Test]
        public void GetLastFrameOfClips_EmptyTimeline_ReturnsZero()
        {
            var timeline = new TimelineObject();

            int lastFrame = timeline.GetLastFrameOfClips();

            Assert.That(lastFrame, Is.EqualTo(0));
        }

        [Test]
        public void GetLastFrameOfClips_WithClips_ReturnsMaximumEndFrame()
        {
            var timeline = new TimelineObject();
            var layer = new LayerObject("layer1", "Layer 1");
            timeline.Layers.Add(layer);

            var clip1 = new ClipObject("clip1") { StartFrame = 0, EndFrame = 50 };
            var clip2 = new ClipObject("clip2") { StartFrame = 60, EndFrame = 150 };
            var clip3 = new ClipObject("clip3") { StartFrame = 200, EndFrame = 300 };

            layer.Objects.Add(clip1);
            layer.Objects.Add(clip2);
            layer.Objects.Add(clip3);

            int lastFrame = timeline.GetLastFrameOfClips();

            Assert.That(lastFrame, Is.EqualTo(300));
        }

        [Test]
        public void GetLastFrameOfClips_MultipleLayers_ReturnsMaximumEndFrame()
        {
            var timeline = new TimelineObject();
            var layer1 = new LayerObject("layer1", "Layer 1");
            var layer2 = new LayerObject("layer2", "Layer 2");

            timeline.Layers.Add(layer1);
            timeline.Layers.Add(layer2);

            layer1.Objects.Add(new ClipObject("clip1") { StartFrame = 0, EndFrame = 100 });
            layer2.Objects.Add(new ClipObject("clip2") { StartFrame = 50, EndFrame = 200 });

            int lastFrame = timeline.GetLastFrameOfClips();

            Assert.That(lastFrame, Is.EqualTo(200));
        }

        [Test]
        public async Task RenderAsync_NestedGroupControls_DoNotDoubleApplyOuterTransform()
        {
            var timeline = new TimelineObject("timeline");

            var outerLayer = new LayerObject("layer-1", "Outer");
            outerLayer.Objects.Add(new GroupControlObject("outer")
            {
                StartFrame = 0,
                EndFrame = 10,
                X = new(10)
            });

            var innerLayer = new LayerObject("layer-2", "Inner");
            innerLayer.Objects.Add(new GroupControlObject("inner")
            {
                StartFrame = 0,
                EndFrame = 10,
                X = new(20)
            });

            var contentLayer = new LayerObject("layer-3", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10,
                X = new(5)
            });

            timeline.Layers.Add(outerLayer);
            timeline.Layers.Add(innerLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(1920, 1080),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var layerNode = (NormalRenderNode)root.Children[0];
            Assert.That(layerNode.Children, Has.Count.EqualTo(1));
            Assert.That(layerNode.Children[0], Is.InstanceOf<NormalRenderNode>());

            var contentNode = (NormalRenderNode)layerNode.Children[0];
            Assert.That(contentNode.Transform.Position.X, Is.EqualTo(35).Within(0.001f));
            Assert.That(contentNode.Transform.Position.Y, Is.EqualTo(0).Within(0.001f));
        }

        [Test]
        public async Task RenderAsync_GroupControlWithDefaultSettings_PreservesChildLogicalSize()
        {
            var timeline = new TimelineObject("timeline");

            var groupLayer = new LayerObject("layer-1", "Group");
            groupLayer.Objects.Add(new GroupControlObject("group")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            var contentLayer = new LayerObject("layer-2", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            timeline.Layers.Add(groupLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var layerNode = (NormalRenderNode)root.Children[0];
            Assert.That(layerNode.Children, Has.Count.EqualTo(1));
            Assert.That(layerNode.Children[0], Is.InstanceOf<NormalRenderNode>());

            var contentNode = (NormalRenderNode)layerNode.Children[0];
            Assert.That(contentNode.LogicalSize.Width, Is.EqualTo(200).Within(0.001f));
            Assert.That(contentNode.LogicalSize.Height, Is.EqualTo(200).Within(0.001f));
            Assert.That(contentNode.Transform.Scale, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        public async Task RenderAsync_GroupControlWithBorderEffect_ExpandsFromChildLogicalSize()
        {
            var timeline = new TimelineObject("timeline");

            var group = new GroupControlObject("group")
            {
                StartFrame = 0,
                EndFrame = 10
            };
            group.VisualEffects.Add(new BorderEffect
            {
                Size = new(5)
            });

            var groupLayer = new LayerObject("layer-1", "Group");
            groupLayer.Objects.Add(group);

            var contentLayer = new LayerObject("layer-2", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            timeline.Layers.Add(groupLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var layerNode = (NormalRenderNode)root.Children[0];
            Assert.That(layerNode.Children, Has.Count.EqualTo(1));
            Assert.That(layerNode.Children[0], Is.InstanceOf<NormalRenderNode>());

            var contentNode = (NormalRenderNode)layerNode.Children[0];
            Assert.That(contentNode.LogicalSize.Width, Is.EqualTo(210).Within(0.001f));
            Assert.That(contentNode.LogicalSize.Height, Is.EqualTo(210).Within(0.001f));
        }

        [Test]
        public async Task RenderAsync_CameraControlWithDefaultSettings_PreservesProjectLogicalSize()
        {
            var timeline = new TimelineObject("timeline");

            var cameraLayer = new LayerObject("layer-1", "Camera");
            cameraLayer.Objects.Add(new CameraControlObject("camera")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            var contentLayer = new LayerObject("layer-2", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            timeline.Layers.Add(cameraLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var cameraNode = (NormalRenderNode)root.Children[0];
            Assert.That(cameraNode.LogicalSize.Width, Is.EqualTo(1920).Within(0.001f));
            Assert.That(cameraNode.LogicalSize.Height, Is.EqualTo(1080).Within(0.001f));
            Assert.That(cameraNode.Transform.Scale, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        public async Task RenderAsync_CameraControlWithVisualEffect_PreservesProjectLogicalSize()
        {
            var timeline = new TimelineObject("timeline");

            var camera = new CameraControlObject("camera")
            {
                StartFrame = 0,
                EndFrame = 10
            };
            camera.VisualEffects.Add(new FlipEffect { FlipHorizontal = true });

            var cameraLayer = new LayerObject("layer-1", "Camera");
            cameraLayer.Objects.Add(camera);

            var contentLayer = new LayerObject("layer-2", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            timeline.Layers.Add(cameraLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var cameraNode = (NormalRenderNode)root.Children[0];
            Assert.That(cameraNode.LogicalSize.Width, Is.EqualTo(1920).Within(0.001f));
            Assert.That(cameraNode.LogicalSize.Height, Is.EqualTo(1080).Within(0.001f));
            Assert.That(cameraNode.Transform.Scale, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        public async Task RenderAsync_CameraControlWithBorderEffect_ExpandsLogicalSizeFromProjectResolution()
        {
            var timeline = new TimelineObject("timeline");

            var camera = new CameraControlObject("camera")
            {
                StartFrame = 0,
                EndFrame = 10
            };
            camera.VisualEffects.Add(new BorderEffect
            {
                Size = new(5)
            });

            var cameraLayer = new LayerObject("layer-1", "Camera");
            cameraLayer.Objects.Add(camera);

            var contentLayer = new LayerObject("layer-2", "Content");
            contentLayer.Objects.Add(new kariHelloObject("content")
            {
                StartFrame = 0,
                EndFrame = 10
            });

            timeline.Layers.Add(cameraLayer);
            timeline.Layers.Add(contentLayer);

            var context = new RenderContext(
                frame: 0,
                projectResolution: new SKSize(1920, 1080),
                renderResolution: new SKSize(960, 540),
                imageFileAccessor: new EmptyImageFileAccessor(),
                videoFileAccessor: new EmptyVideoFileAccessor(),
                projectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2),
                projectPath: string.Empty);

            var result = await timeline.RenderAsync(context);

            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var root = (NormalRenderNode)result;
            Assert.That(root.Children, Has.Count.EqualTo(1));
            Assert.That(root.Children[0], Is.InstanceOf<NormalRenderNode>());

            var cameraNode = (NormalRenderNode)root.Children[0];
            Assert.That(cameraNode.LogicalSize.Width, Is.EqualTo(1930).Within(0.001f));
            Assert.That(cameraNode.LogicalSize.Height, Is.EqualTo(1090).Within(0.001f));
        }
    }
}
