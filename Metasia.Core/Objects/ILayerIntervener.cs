using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Objects;

public interface ILayerIntervener
{
    /// <summary>
    /// 対象レイヤーの指定
    /// 無限または特定のレイヤー数を設定可能
    /// </summary>
    LayerTarget TargetLayers { get; }
}
