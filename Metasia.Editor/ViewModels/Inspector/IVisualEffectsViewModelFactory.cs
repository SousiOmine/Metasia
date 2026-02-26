using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Inspector;

public interface IVisualEffectsViewModelFactory
{
    VisualEffectsViewModel Create(IRenderable target);
}
