using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface ILayerTargetPropertyViewModelFactory
{
    LayerTargetPropertyViewModel Create(string propertyIdentifier, LayerTarget target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}
