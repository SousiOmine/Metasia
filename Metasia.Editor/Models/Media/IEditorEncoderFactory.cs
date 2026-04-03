using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
namespace Metasia.Editor.Models.Media;

public interface IEditorEncoderFactory
{
    string Name { get; }
    string[] SupportedExtensions { get; }
    IEditorEncoder CreateEncoder();
}
