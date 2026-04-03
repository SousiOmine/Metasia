using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
namespace Metasia.Editor.Services
{
    public interface IClipboardService
    {
        bool HasClips { get; }

        void StoreClips(string clipTemplateXml);

        string? GetStoredClips();

        void Clear();
    }
}