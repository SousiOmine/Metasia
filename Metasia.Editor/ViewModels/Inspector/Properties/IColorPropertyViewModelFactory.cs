using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters.Color;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IColorPropertyViewModelFactory
{
    ColorPropertyViewModel Create(string propertyIdentifier, ColorRgb8 target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}
