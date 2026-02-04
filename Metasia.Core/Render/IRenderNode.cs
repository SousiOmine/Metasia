namespace Metasia.Core.Render;

public interface IRenderNode
{
    Transform Transform { get; set; }
    IReadOnlyList<IRenderNode> Children { get; }
}
