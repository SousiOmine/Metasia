using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IStringPropertyViewModelFactory
{
    StringPropertyViewModel Create(string propertyIdentifier, string target);
}