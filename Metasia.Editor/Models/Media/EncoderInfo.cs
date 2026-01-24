namespace Metasia.Editor.Models.Media;

public class EncoderInfo
{
    public string Name { get; }
    public string OriginName { get; }
    public string[] SupportedExtensions { get; }
    public IEditorEncoderFactory Factory { get; }

    public EncoderInfo(string name, string originName, string[] supportedExtensions, IEditorEncoderFactory factory)
    {
        Name = name;
        OriginName = originName;
        SupportedExtensions = supportedExtensions;
        Factory = factory;
    }

    public string DisplayName => $"{Name} ({OriginName})";
}
