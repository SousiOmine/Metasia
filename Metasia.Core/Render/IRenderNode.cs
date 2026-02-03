namespace Metasia.Core.Render;

public interface IRenderNode
{
    IReadOnlyList<IRenderNode> Children { get; }
}
