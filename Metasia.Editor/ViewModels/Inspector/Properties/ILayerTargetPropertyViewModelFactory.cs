using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface ILayerTargetPropertyViewModelFactory
{
    LayerTargetPropertyViewModel Create(string propertyIdentifier, LayerTarget target);
}
