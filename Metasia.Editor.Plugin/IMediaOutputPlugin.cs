using Metasia.Core.Encode;

namespace Metasia.Editor.Plugin;

public interface IMediaOutputPlugin : IEditorPlugin
{
    EncoderBase Encoder { get; }

    /// <summary>
    /// エンコーダーの名前として表示する文字列
    /// </summary>
    string Name { get; }

    /// <summary>
    /// サポートするファイル拡張子の配列 *.mp4, *.png, *.avi など
    /// </summary>
    string[] SupportedExtensions { get; }
}