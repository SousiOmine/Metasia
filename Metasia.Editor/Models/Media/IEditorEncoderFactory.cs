namespace Metasia.Editor.Models.Media;

public interface IEditorEncoderFactory
{
    string Name { get; }
    string[] SupportedExtensions { get; }
    IEditorEncoder CreateEncoder();
}
