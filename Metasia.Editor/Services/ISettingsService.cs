using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Threading.Tasks;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.Services
{
    public interface ISettingsService
    {
        EditorSettings CurrentSettings { get; }
        Task LoadAsync();
        Task SaveAsync();
        void UpdateSettings(EditorSettings settings);
        Task UpdateSettingsAsync(EditorSettings settings);
        void UpdateSettingsSilent(EditorSettings settings);
        void NotifySettingsChanged();
        event Action? SettingsChanged;
    }
}
