using Metasia.Core.Objects;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using NUnit.Framework;
using Metasia.Core.Render;
using System.Threading.Tasks;

namespace Metasia.Core.Tests.Objects;

[TestFixture]
public class ShapeObjectTests
{
    [Test]
    public void Constructor_WithId_SetsId()
    {
        // Arrange
        string id = "test-id";

        // Act
        var obj = new ShapeObject(id);

        // Assert
        Assert.That(obj.Id, Is.EqualTo(id));
    }

    [Test]
    public void DefaultConstructor_SetsDefaultShape()
    {
        // Arrange & Act
        var obj = new ShapeObject();

        // Assert
        Assert.That(obj.Shape.SelectedValue, Is.EqualTo("Circle"));
    }

    [Test]
    public void SetShape_SetsCorrectValue()
    {
        // Arrange
        var obj = new ShapeObject();

        // Act
        obj.Shape.SelectedValue = "Square";

        // Assert
        Assert.That(obj.Shape.SelectedValue, Is.EqualTo("Square"));
    }

    [Test]
    public async Task RenderAsync_ReturnsRenderNode()
    {
        // Arrange
        var obj = new ShapeObject("test-obj");
        obj.Shape.SelectedValue = "Triangle";

        var context = new RenderContext(
            0,
            new SkiaSharp.SKSize(1920, 1080),
            new SkiaSharp.SKSize(1920, 1080),
            null,
            null,
            new Core.Project.ProjectInfo(),
            string.Empty
        );

        // Act
        var result = await obj.RenderAsync(context);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<NormalRenderNode>());
        var normalNode = (NormalRenderNode)result;
        Assert.That(normalNode.Image, Is.Not.Null);
        Assert.That(normalNode.LogicalSize.Width, Is.GreaterThan(0));
        Assert.That(normalNode.LogicalSize.Height, Is.GreaterThan(0));
    }

    [Test]
    public void SplitAtFrame_CreatesTwoCorrectObjects()
    {
        // Arrange
        var obj = new ShapeObject("test-obj");
        obj.Shape.SelectedValue = "Star";
        obj.X = new MetaNumberParam<double>(100);
        obj.Y = new MetaNumberParam<double>(200);

        int splitFrame = 10;

        // Act
        var (firstClip, secondClip) = obj.SplitAtFrame(splitFrame);

        // Assert
        Assert.That(firstClip, Is.TypeOf<ShapeObject>());
        Assert.That(secondClip, Is.TypeOf<ShapeObject>());

        var firstObject = (ShapeObject)firstClip;
        var secondObject = (ShapeObject)secondClip;

        Assert.That(firstObject.Shape.SelectedValue, Is.EqualTo("Star"));
        Assert.That(secondObject.Shape.SelectedValue, Is.EqualTo("Star"));

        Assert.That(firstObject.Id, Is.EqualTo("test-obj_part1"));
        Assert.That(secondObject.Id, Is.EqualTo("test-obj_part2"));
    }

    [Test]
    public void CreateCopy_CreatesCopyWithCorrectId()
    {
        // Arrange
        var obj = new ShapeObject("test-obj");
        obj.Shape.SelectedValue = "Square";

        // Act
        var xml = Metasia.Core.Xml.MetasiaObjectXmlSerializer.Serialize(obj);
        var copy = Metasia.Core.Xml.MetasiaObjectXmlSerializer.Deserialize<ShapeObject>(xml);

        // Assert
        Assert.That(copy.Id, Is.EqualTo("test-obj")); // Serialized object retains original ID
        Assert.That(copy.Shape.SelectedValue, Is.EqualTo("Square"));
    }

    [Test]
    public void ShapeOptions_ContainsAllExpectedShapes()
    {
        // Arrange
        var obj = new ShapeObject();

        // Act & Assert
        var options = obj.Shape.Options.ToList();
        Assert.That(options, Contains.Item("Circle"));
        Assert.That(options, Contains.Item("Square"));
        Assert.That(options, Contains.Item("Triangle"));
        Assert.That(options, Contains.Item("Star"));
        Assert.That(options.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task RenderAsync_WithDifferentShapes_RendersCorrectly()
    {
        // Arrange
        var shapes = new[] { "Circle", "Square", "Triangle", "Star" };
        var context = new RenderContext(
            0,
            new SkiaSharp.SKSize(1920, 1080),
            new SkiaSharp.SKSize(1920, 1080),
            null,
            null,
            new Core.Project.ProjectInfo(),
            string.Empty
        );

        // Act & Assert
        foreach (var shape in shapes)
        {
            var obj = new ShapeObject($"test-obj-{shape}");
            obj.Shape.SelectedValue = shape;

            var result = await obj.RenderAsync(context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<NormalRenderNode>());
            var normalNode = (NormalRenderNode)result;
            Assert.That(normalNode.Image, Is.Not.Null);
            Assert.That(normalNode.LogicalSize.Width, Is.GreaterThan(0));
            Assert.That(normalNode.LogicalSize.Height, Is.GreaterThan(0));
        }
    }
}
