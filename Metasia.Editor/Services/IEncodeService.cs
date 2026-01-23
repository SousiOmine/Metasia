using System.Collections.Generic;
using Metasia.Core.Encode;
using Metasia.Editor.Models.Media;

namespace Metasia.Editor.Services;

/// <summary>
/// エディタ内でエンコーダを管理したりする
/// </summary>
public interface IEncodeService
{
    IReadOnlyList<IEditorEncoder> Encoders { get; }

    void QueueEncode(IEditorEncoder encoder, string outputPath);
    void Cancel(IEditorEncoder encoder);
    void Delete(IEditorEncoder encoder);
    void ClearQueue();
}
