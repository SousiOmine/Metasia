using System.Xml;
using System.Xml.Serialization;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

public class UnknownVisualEffect : VisualEffectBase, IUnknownMetasiaObject
{
    [XmlAnyElement]
    public XmlElement[] RawElements { get; set; } = Array.Empty<XmlElement>();

    public string RawXml
    {
        get => RawElements.Length > 0 ? RawElements[0].OuterXml : string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                RawElements = Array.Empty<XmlElement>();
                return;
            }

            var document = new XmlDocument();
            document.LoadXml(value);
            RawElements = [document.DocumentElement!];
        }
    }

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        return new VisualEffectResult(input, context.TargetImageCacheKey);
    }
}
