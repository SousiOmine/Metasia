using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

public class UnknownVisualEffect : VisualEffectBase, IUnknownMetasiaObject, IXmlSerializable
{
    public string RawXml { get; set; } = string.Empty;

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        return new VisualEffectResult(input, context.TargetImageCacheKey);
    }

    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        RawXml = reader.ReadOuterXml();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteRaw(RawXml);
    }
}