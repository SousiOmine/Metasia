using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;


using System;
using Metasia.Core.Encode;
using Metasia.Core.Render;
using Metasia.Editor.Plugin;
namespace Metasia.Editor.Models.Media;

public interface IEditorEncoder : IEncoder, IDisposable
{
    string Name { get; }

    /// <summary>
    /// 対応するファイル拡張子の配列 *.avi, *.mp4 など
    /// </summary>
    string[] SupportedExtensions { get; }

    /// <summary>
    /// 出力パス
    /// </summary>
    string OutputPath { get; }

    /// <summary>
    /// フレームレンダリングに使用するサーフェスファクトリ（null の場合は CPU フォールバック）
    /// </summary>
    IRenderSurfaceFactory? SurfaceFactory { get; set; }
}
