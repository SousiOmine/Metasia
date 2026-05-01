using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Media;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMediaPathPropertyViewModelFactory
{
    MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}
