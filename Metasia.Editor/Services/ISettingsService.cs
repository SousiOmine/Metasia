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
        Task UpdateSettingsAsync(EditorSettings settings);
        event Action? SettingsChanged;
    }
}
