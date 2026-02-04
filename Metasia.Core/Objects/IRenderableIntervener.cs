using Metasia.Core.Render;

namespace Metasia.Core.Objects;

public interface IRenderableIntervener : ILayerIntervener
{
    Task<IRenderNode> ApplyControlAsync(IReadOnlyList<IRenderNode> targetNodes, RenderContext context, CancellationToken cancellationToken = default);
}