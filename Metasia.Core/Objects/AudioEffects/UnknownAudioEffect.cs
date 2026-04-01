using System.Xml;
using System.Xml.Serialization;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects.AudioEffects;

public class UnknownAudioEffect : AudioEffectBase, IUnknownMetasiaObject
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

    public override IAudioChunk Apply(IAudioChunk input, AudioEffectContext context)
    {
        return input;
    }
}
