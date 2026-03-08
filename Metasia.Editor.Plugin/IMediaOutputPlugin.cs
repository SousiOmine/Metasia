using Avalonia.Controls;
using Metasia.Core.Encode;

namespace Metasia.Editor.Plugin;

public interface IMediaOutputPlugin : IEditorPlugin
{
    /// <summary>
    /// エンコーダーの名前として表示する文字列
    /// </summary>
    string Name { get; }

    /// <summary>
    /// サポートするファイル拡張子の配列 *.mp4, *.png, *.avi など
    /// </summary>
    string[] SupportedExtensions { get; }

    IMediaOutputSession CreateSession();
}

public interface IMediaOutputSession : IDisposable
{
    string Name { get; }
    string[] SupportedExtensions { get; }
    Control? SettingsView { get; }
    EncoderBase CreateEncoderInstance();
}
