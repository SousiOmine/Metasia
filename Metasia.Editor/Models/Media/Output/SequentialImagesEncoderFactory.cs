namespace Metasia.Editor.Models.Media.Output;

using Metasia.Editor.Models.Media;

public class SequentialImagesEncoderFactory : IEditorEncoderFactory
{
    public string Name => "連番画像出力";

    public string[] SupportedExtensions => ["*.png", "*.bmp", "*.jpg", "*.jpeg"];

    public IEditorEncoder CreateEncoder()
    {
        return new SequentialImagesEncoder();
    }
}
