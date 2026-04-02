using System.Xml;
using System.Xml.Serialization;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

public class UnknownVisualEffect : VisualEffectBase, IUnknownMetasiaObject
{
    private const string UnknownXsiTypeAttributeName = "unknownXsiType";
    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    [XmlAnyAttribute]
    public XmlAttribute[] RawAttributes { get; set; } = Array.Empty<XmlAttribute>();

    [XmlAnyElement]
    public XmlElement[] RawElements { get; set; } = Array.Empty<XmlElement>();

    public string RawXml
    {
        get
        {
            var document = new XmlDocument();
            var root = document.CreateElement(nameof(VisualEffectBase));

            foreach (var attribute in RawAttributes ?? Array.Empty<XmlAttribute>())
            {
                AppendAttribute(document, root, attribute);
            }

            AppendElement(document, root, nameof(Id), Id);
            AppendElement(document, root, nameof(IsActive), IsActive.ToString().ToLowerInvariant());

            foreach (var element in RawElements ?? Array.Empty<XmlElement>())
            {
                root.AppendChild(document.ImportNode(element, true));
            }

            return root.OuterXml;
        }
    }

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        return new VisualEffectResult(input, context.TargetImageCacheKey);
    }

    private static void AppendElement(XmlDocument document, XmlElement parent, string name, string value)
    {
        var element = document.CreateElement(name);
        element.InnerText = value;
        parent.AppendChild(element);
    }

    private static void AppendAttribute(XmlDocument document, XmlElement parent, XmlAttribute attribute)
    {
        if (attribute.LocalName == UnknownXsiTypeAttributeName && string.IsNullOrEmpty(attribute.NamespaceURI))
        {
            var xsiTypeAttribute = document.CreateAttribute("xsi", "type", XsiNamespace);
            xsiTypeAttribute.Value = attribute.Value;
            parent.Attributes.Append(xsiTypeAttribute);
            return;
        }

        parent.Attributes.Append((XmlAttribute)document.ImportNode(attribute, true));
    }
}
