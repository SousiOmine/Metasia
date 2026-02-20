using System;
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

    /// <summary>
    /// キューに追加されたり削除されたりしたときに発行されるイベント
    /// </summary>
    event EventHandler<EventArgs> QueueUpdated;

    void QueueEncode(IEditorEncoder encoder);
    void Cancel(IEditorEncoder encoder);
    void Delete(IEditorEncoder encoder);
    void ClearQueue();
}
