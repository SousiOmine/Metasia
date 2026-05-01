using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IStringPropertyViewModelFactory
{
    StringPropertyViewModel Create(string propertyIdentifier, string target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}