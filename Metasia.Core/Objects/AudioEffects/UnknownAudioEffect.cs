using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects.AudioEffects;

public class UnknownAudioEffect : AudioEffectBase, IUnknownMetasiaObject, IXmlSerializable
{
    public string RawXml { get; set; } = string.Empty;

    public override IAudioChunk Apply(IAudioChunk input, AudioEffectContext context)
    {
        return input;
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
