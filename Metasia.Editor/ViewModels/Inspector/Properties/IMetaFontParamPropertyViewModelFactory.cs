using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaFontParamPropertyViewModelFactory
{
    MetaFontParamPropertyViewModel Create(string propertyIdentifier, MetaFontParam target);
}
