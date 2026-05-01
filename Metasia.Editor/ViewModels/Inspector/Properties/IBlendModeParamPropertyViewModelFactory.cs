using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Render;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IBlendModeParamPropertyViewModelFactory
{
    BlendModeParamPropertyViewModel Create(string propertyIdentifier, BlendModeParam target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}