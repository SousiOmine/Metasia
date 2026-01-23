using System.Collections.Generic;
using Metasia.Core.Encode;

namespace Metasia.Editor.Services;

/// <summary>
/// エディタ内でエンコーダを管理したりする
/// </summary>
public interface IEncodeService
{
    IReadOnlyList<IEncoder> Encoders { get; }

    void QueueEncode(IEncoder encoder);
    void Cancel(IEncoder encoder);
    void Delete(IEncoder encoder);
    void ClearQueue();
}
