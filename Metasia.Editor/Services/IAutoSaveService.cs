using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
namespace Metasia.Editor.Services
{
    public interface IAutoSaveService
    {
        void Start();
        void Stop();
        void StartBackup();
        void StopBackup();
    }
}