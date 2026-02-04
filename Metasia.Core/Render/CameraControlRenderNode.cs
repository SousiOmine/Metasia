namespace Metasia.Core.Render;

/// <summary>
/// 下位レイヤーに対してカメラ制御を行うことができる
/// </summary>
public class CameraControlRenderNode : IRenderNode
{
    public int ScopeLayerCount { get; init; }
    public Transform Transform { get; set; } = Transform.Identity;
    public IReadOnlyList<IRenderNode> Children { get; set; } = [];
}