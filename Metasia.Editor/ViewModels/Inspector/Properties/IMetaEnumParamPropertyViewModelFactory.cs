using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models;
using Metasia.Editor.Abstractions.States;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaEnumParamPropertyViewModelFactory
{
    MetaEnumParamPropertyViewModel Create(string propertyIdentifier, MetaEnumParam target);
}
