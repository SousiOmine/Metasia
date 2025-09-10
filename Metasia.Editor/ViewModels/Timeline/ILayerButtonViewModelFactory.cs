using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Timeline;

public interface ILayerButtonViewModelFactory
{
    LayerButtonViewModel Create(LayerObject targetLayerObject);
}
