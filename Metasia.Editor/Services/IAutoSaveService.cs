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