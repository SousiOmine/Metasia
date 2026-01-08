using System;
using NUnit.Framework;
using Metasia.Core.Xml;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;
using System.Xml;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Tests.Xml;

[TestFixture]
public class MetasiaObjectXmlSerializerTests
{
    // ClipObjectのシリアライズ/デシリアライズテスト (基本的なIMetasiaObject実装クラス)
    // 意図: 基本的なオブジェクトが正しくシリアライズ/デシリアライズできることを確認
    [Test]
    public void SerializeDeserialize_ClipObject_Success()
    {
        // Arrange
        var original = new ClipObject("test_id")
        {
            StartFrame = 10,
            EndFrame = 20,
            IsActive = true
        };

        // Act
        string xml = MetasiaObjectXmlSerializer.Serialize(original);
        var deserialized = MetasiaObjectXmlSerializer.Deserialize<ClipObject>(xml);

        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Id, Is.EqualTo(original.Id));
        Assert.That(deserialized.StartFrame, Is.EqualTo(original.StartFrame));
        Assert.That(deserialized.EndFrame, Is.EqualTo(original.EndFrame));
        Assert.That(deserialized.IsActive, Is.EqualTo(original.IsActive));
    }

    // TimelineObjectのシリアライズ/デシリアライズテスト (複雑なIMetasiaObject実装クラス)
    // 意図: 複雑なプロパティを持つオブジェクトが正しくシリアライズ/デシリアライズできることを確認
    [Test]
    public void SerializeDeserialize_TimelineObject_Success()
    {
        // Arrange
        var original = new TimelineObject("timeline_id")
        {
            SelectionStart = 0,
            SelectionEnd = 100,
            IsActive = true,
            Volume = 80.0
        };

        // LayerObjectを追加
        var layer = new LayerObject("layer_id", "TestLayer");
        original.Layers.Add(layer);

        // Act
        string xml = MetasiaObjectXmlSerializer.Serialize(original);
        var deserialized = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xml);

        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Id, Is.EqualTo(original.Id));
        Assert.That(deserialized.SelectionStart, Is.EqualTo(original.SelectionStart));
        Assert.That(deserialized.SelectionEnd, Is.EqualTo(original.SelectionEnd));
        Assert.That(deserialized.IsActive, Is.EqualTo(original.IsActive));
        Assert.That(deserialized.Volume, Is.EqualTo(original.Volume));
        Assert.That(deserialized.Layers, Is.Not.Null);
        Assert.That(deserialized.Layers.Count, Is.EqualTo(1));
        Assert.That(deserialized.Layers[0].Id, Is.EqualTo("layer_id"));
        Assert.That(deserialized.Layers[0].Name, Is.EqualTo("TestLayer"));
    }

    // Textオブジェクトのシリアライズ/デシリアライズテスト (特殊なプロパティを持つIMetasiaObject実装クラス)
    // 意図: 特殊な型のプロパティを持つオブジェクトが正しくシリアライズ/デシリアライズできることを確認
    [Test]
    public void SerializeDeserialize_TextObject_Success()
    {
        // Arrange
        var original = new Text("text_id")
        {
            StartFrame = 15,
            EndFrame = 30,
            IsActive = true,
            Contents = "Test Text",
            TextSize = new MetaNumberParam<double>(12.0),
            Font = new MetaFontParam("Arial", true, false)
        };

        // Act
        string xml = MetasiaObjectXmlSerializer.Serialize(original);
        var deserialized = MetasiaObjectXmlSerializer.Deserialize<Text>(xml);

        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Id, Is.EqualTo(original.Id));
        Assert.That(deserialized.StartFrame, Is.EqualTo(original.StartFrame));
        Assert.That(deserialized.EndFrame, Is.EqualTo(original.EndFrame));
        Assert.That(deserialized.IsActive, Is.EqualTo(original.IsActive));
        Assert.That(deserialized.Contents, Is.EqualTo(original.Contents));
        Assert.That(deserialized.Font.FamilyName, Is.EqualTo(original.Font.FamilyName));
        Assert.That(deserialized.Font.IsBold, Is.EqualTo(original.Font.IsBold));
        Assert.That(deserialized.Font.IsItalic, Is.EqualTo(original.Font.IsItalic));
    }

    // nullオブジェクトをシリアライズしようとしたときの例外処理テスト
    // 意図: nullオブジェクトをシリアライズした場合にArgumentNullExceptionがスローされることを確認
    [Test]
    public void Serialize_NullObject_ThrowsArgumentNullException()
    {
        // Arrange
        ClipObject? nullObject = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MetasiaObjectXmlSerializer.Serialize(nullObject!));
    }

    // 空のXMLをデシリアライズしようとしたときの例外処理テスト
    // 意図: 空のXMLをデシリアライズした場合にArgumentExceptionがスローされることを確認
    [Test]
    public void Deserialize_EmptyXml_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => MetasiaObjectXmlSerializer.Deserialize<ClipObject>(string.Empty));
    }

    // 不正なXMLをデシリアライズしようとしたときの例外処理テスト
    // 意図: 不正なXMLをデシリアライズした場合にInvalidOperationExceptionがスローされることを確認
    [Test]
    public void Deserialize_InvalidXml_ThrowsInvalidOperationException()
    {
        // Arrange
        string invalidXml = "<InvalidXml>";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MetasiaObjectXmlSerializer.Deserialize<ClipObject>(invalidXml));
    }
}
