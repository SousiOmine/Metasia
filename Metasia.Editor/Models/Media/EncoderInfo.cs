namespace Metasia.Editor.Models.Media;

public class EncoderInfo
{
    public string Name { get; set; } = string.Empty;
    public string OriginName { get; set; } = string.Empty;
    public string[] SupportedExtensions { get; set; } = [];
    public IEditorEncoderFactory Factory { get; set; } = null!;

    public string DisplayName => $"{Name} ({OriginName})";
}
