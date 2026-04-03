using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Media;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMediaPathPropertyViewModelFactory
{
    MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target);
}
