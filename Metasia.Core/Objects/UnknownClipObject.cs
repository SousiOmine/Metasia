using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Metasia.Core.Objects;

public class UnknownClipObject : ClipObject, IUnknownMetasiaObject, IXmlSerializable
{
    public string RawXml { get; set; } = string.Empty;

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