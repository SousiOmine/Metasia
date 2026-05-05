using System;
using NUnit.Framework;
using Metasia.Core.Xml;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Coordinate;
using System.Xml;
using Metasia.Core.Objects.Parameters;
using System.Xml.Linq;
using Metasia.Core.Objects.AudioEffects;

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

    [Test]
    public void SerializeDeserialize_TimelineReferenceObject_Success()
    {
        var original = new TimelineReferenceObject("timeline_ref_id")
        {
            StartFrame = 12,
            EndFrame = 48,
            IsActive = true,
            TargetTimelineId = "TargetTimeline",
            SourceStartFrame = new MetaDoubleParam(15),
            Volume = new MetaDoubleParam(80)
        };

        string xml = MetasiaObjectXmlSerializer.Serialize(original);
        var deserialized = MetasiaObjectXmlSerializer.Deserialize<TimelineReferenceObject>(xml);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Id, Is.EqualTo(original.Id));
        Assert.That(deserialized.TargetTimelineId, Is.EqualTo(original.TargetTimelineId));
        Assert.That(deserialized.SourceStartFrame.Value, Is.EqualTo(15).Within(0.001));
        Assert.That(deserialized.Volume.Value, Is.EqualTo(80).Within(0.001));
    }

    [Test]
    public void Serialize_TimelineWithDerivedClip_WritesDerivedTypeIdAndTypeKind()
    {
        var timeline = new TimelineObject("timeline_id");
        var layer = new LayerObject("layer_id", "Layer");
        layer.Objects.Add(new Text("text_id") { Contents = "hello" });
        timeline.Layers.Add(layer);

        string xml = MetasiaObjectXmlSerializer.Serialize(timeline);
        var doc = XDocument.Parse(xml);
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        var clipElement = doc.Descendants("ClipObject").Single();

        Assert.That(clipElement.Attribute("typeId")?.Value, Is.EqualTo("metasia/core:Text"));
        Assert.That(clipElement.Attribute("typeKind")?.Value, Is.EqualTo("Clip"));
        Assert.That(clipElement.Attribute(xsi + "type")?.Value, Is.EqualTo("Text"));
    }

    [Test]
    public void SerializeDeserialize_TimelineWithUnknownClip_PreservesOriginalXml()
    {
        const string xml = """
            <TimelineObject xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" typeId="metasia/core:TimelineObject">
              <Id>timeline_id</Id>
              <IsActive>true</IsActive>
              <SelectionStart>0</SelectionStart>
              <SelectionEnd>100</SelectionEnd>
              <Layers>
                <LayerObject typeId="metasia/core:LayerObject">
                  <Id>layer_id</Id>
                  <IsActive>true</IsActive>
                  <Objects>
                    <ClipObject xsi:type="PluginText" typeId="plugin:test:PluginText" typeKind="Clip">
                      <StartFrame>10</StartFrame>
                      <EndFrame>20</EndFrame>
                      <Id>plugin_clip</Id>
                      <IsActive>true</IsActive>
                    </ClipObject>
                  </Objects>
                  <Volume Value="100" />
                  <AudioEffects />
                  <VisualEffects />
                  <Name>Layer</Name>
                </LayerObject>
              </Layers>
              <Volume Value="100" />
              <AudioEffects />
              <VisualEffects />
            </TimelineObject>
            """;

        var deserialized = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xml);

        Assert.That(deserialized.Layers[0].Objects[0], Is.TypeOf<UnknownClipObject>());
        var unknownClip = (UnknownClipObject)deserialized.Layers[0].Objects[0];
        Assert.That(unknownClip.StartFrame, Is.EqualTo(10));
        Assert.That(unknownClip.EndFrame, Is.EqualTo(20));
        Assert.That(unknownClip.Id, Is.EqualTo("plugin_clip"));
        Assert.That(unknownClip.IsActive, Is.True);
        Assert.That(unknownClip.RawXml, Does.Contain("PluginText"));

        string serialized = MetasiaObjectXmlSerializer.Serialize(deserialized);
        var doc = XDocument.Parse(serialized);
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        Assert.That(doc.Descendants(nameof(UnknownClipObject)), Is.Empty);

        var clipElement = doc.Descendants("ClipObject").Single();
        Assert.That(clipElement.Attribute("typeId")?.Value, Is.EqualTo("plugin:test:PluginText"));
        Assert.That(clipElement.Attribute("typeKind")?.Value, Is.EqualTo("Clip"));
        Assert.That(clipElement.Attribute(xsi + "type")?.Value, Is.EqualTo("PluginText"));
        Assert.That((int?)clipElement.Element(nameof(ClipObject.StartFrame)), Is.EqualTo(10));
        Assert.That((int?)clipElement.Element(nameof(ClipObject.EndFrame)), Is.EqualTo(20));
        Assert.That((string?)clipElement.Element(nameof(ClipObject.Id)), Is.EqualTo("plugin_clip"));
    }

    [Test]
    public void Serialize_TimelineWithEditedUnknownClip_PersistsSharedPropertiesAndUnknownPayload()
    {
        const string xml = """
            <TimelineObject xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" typeId="metasia/core:TimelineObject">
              <Id>timeline_id</Id>
              <IsActive>true</IsActive>
              <SelectionStart>0</SelectionStart>
              <SelectionEnd>100</SelectionEnd>
              <Layers>
                <LayerObject typeId="metasia/core:LayerObject">
                  <Id>layer_id</Id>
                  <IsActive>true</IsActive>
                  <Objects>
                    <ClipObject xsi:type="PluginText" typeId="plugin:test:PluginText" typeKind="Clip">
                      <StartFrame>10</StartFrame>
                      <EndFrame>20</EndFrame>
                      <Id>plugin_clip</Id>
                      <IsActive>true</IsActive>
                      <PluginTextColor>#00FF00</PluginTextColor>
                    </ClipObject>
                  </Objects>
                  <Volume Value="100" />
                  <AudioEffects />
                  <VisualEffects />
                  <Name>Layer</Name>
                </LayerObject>
              </Layers>
              <Volume Value="100" />
              <AudioEffects />
              <VisualEffects />
            </TimelineObject>
            """;

        var deserialized = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xml);
        var unknownClip = (UnknownClipObject)deserialized.Layers[0].Objects[0];
        unknownClip.StartFrame = 30;
        unknownClip.EndFrame = 45;
        unknownClip.IsActive = false;

        string serialized = MetasiaObjectXmlSerializer.Serialize(deserialized);
        var clipElement = XDocument.Parse(serialized).Descendants(nameof(ClipObject)).Single();

        Assert.That((int?)clipElement.Element(nameof(ClipObject.StartFrame)), Is.EqualTo(30));
        Assert.That((int?)clipElement.Element(nameof(ClipObject.EndFrame)), Is.EqualTo(45));
        Assert.That((bool?)clipElement.Element(nameof(ClipObject.IsActive)), Is.False);
        Assert.That((string?)clipElement.Element("PluginTextColor"), Is.EqualTo("#00FF00"));
    }

    [Test]
    public void Serialize_TimelineWithEditedUnknownAudioEffect_PersistsSharedPropertiesAndUnknownPayload()
    {
        const string xml = """
            <TimelineObject xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" typeId="metasia/core:TimelineObject">
              <Id>timeline_id</Id>
              <IsActive>true</IsActive>
              <SelectionStart>0</SelectionStart>
              <SelectionEnd>100</SelectionEnd>
              <Layers>
                <LayerObject typeId="metasia/core:LayerObject">
                  <Id>layer_id</Id>
                  <IsActive>true</IsActive>
                  <Objects />
                  <Volume Value="100" />
                  <AudioEffects>
                    <AudioEffectBase xsi:type="PluginVolume" typeId="plugin:test:PluginVolume" typeKind="AudioEffect">
                      <Id>plugin_audio_effect</Id>
                      <IsActive>true</IsActive>
                      <PluginGain Value="6" />
                    </AudioEffectBase>
                  </AudioEffects>
                  <VisualEffects />
                  <Name>Layer</Name>
                </LayerObject>
              </Layers>
              <Volume Value="100" />
              <AudioEffects />
              <VisualEffects />
            </TimelineObject>
            """;

        var deserialized = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xml);
        Assert.That(deserialized.Layers[0].AudioEffects[0], Is.TypeOf<UnknownAudioEffect>());

        var unknownEffect = (UnknownAudioEffect)deserialized.Layers[0].AudioEffects[0];
        unknownEffect.IsActive = false;
        unknownEffect.Id = "edited_audio_effect";

        string serialized = MetasiaObjectXmlSerializer.Serialize(deserialized);
        var effectElement = XDocument.Parse(serialized).Descendants(nameof(AudioEffectBase)).Single();
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        Assert.That(effectElement.Attribute("typeId")?.Value, Is.EqualTo("plugin:test:PluginVolume"));
        Assert.That(effectElement.Attribute("typeKind")?.Value, Is.EqualTo("AudioEffect"));
        Assert.That(effectElement.Attribute(xsi + "type")?.Value, Is.EqualTo("PluginVolume"));
        Assert.That((string?)effectElement.Element(nameof(AudioEffectBase.Id)), Is.EqualTo("edited_audio_effect"));
        Assert.That((bool?)effectElement.Element(nameof(AudioEffectBase.IsActive)), Is.False);
        Assert.That(effectElement.Element("PluginGain")?.Attribute("Value")?.Value, Is.EqualTo("6"));
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
    // 意図: 不正なXMLをデシリアライズした場合にXmlExceptionがスローされることを確認
    [Test]
    public void Deserialize_InvalidXml_ThrowsXmlException()
    {
        // Arrange
        string invalidXml = "<InvalidXml>";

        // Act & Assert
        Assert.Throws<XmlException>(() => MetasiaObjectXmlSerializer.Deserialize<ClipObject>(invalidXml));
    }
}
