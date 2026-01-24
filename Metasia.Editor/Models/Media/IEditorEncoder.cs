

using System;
using Metasia.Core.Encode;
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

    void SetOutputPath(string outputPath);
}
