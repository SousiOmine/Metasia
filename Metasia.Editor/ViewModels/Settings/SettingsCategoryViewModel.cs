using System;
using ReactiveUI;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.ViewModels.Settings
{
    public abstract class SettingsCategoryViewModel : ViewModelBase
    {
        protected EditorSettings _settings;

        public abstract string Name { get; }

        public event Action? SettingsEdited;

        protected SettingsCategoryViewModel(EditorSettings settings)
        {
            _settings = settings;
        }

        public void UpdateSettings(EditorSettings settings)
        {
            _settings = settings;
            OnSettingsUpdated();
        }

        protected void NotifySettingsEdited()
        {
            SettingsEdited?.Invoke();
        }

        protected abstract void OnSettingsUpdated();
    }
}
