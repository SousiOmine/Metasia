using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Render;

/// <summary>
/// 下位レイヤーに対してカメラ制御を行うことができる
/// </summary>
public class CameraControlRenderNode : IRenderNode
{
    /// <summary>
    /// 対象レイヤーの指定
    /// </summary>
    public LayerTarget ScopeLayerTarget { get; init; } = new LayerTarget();
    public Transform Transform { get; set; } = Transform.Identity;
    public IReadOnlyList<IRenderNode> Children { get; set; } = [];
}
