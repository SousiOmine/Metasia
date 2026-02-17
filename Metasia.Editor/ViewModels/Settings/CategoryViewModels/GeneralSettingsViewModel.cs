using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.ViewModels.Settings
{
    public class GeneralSettingsViewModel : SettingsCategoryViewModel
    {
        public override string Name => "General";

        public IReadOnlyList<SettingOption> LanguageOptions { get; } =
        [
            new SettingOption("ja", "Japanese"),
            new SettingOption("en", "English")
        ];

        public IReadOnlyList<SettingOption> ThemeOptions { get; } =
        [
            new SettingOption("auto", "Auto"),
            new SettingOption("dark", "Dark"),
            new SettingOption("light", "Light")
        ];

        public SettingOption SelectedLanguage
        {
            get => LanguageOptions.FirstOrDefault(option => option.Value == Language) ?? LanguageOptions[0];
            set
            {
                if (value is null)
                {
                    return;
                }

                if (Language != value.Value)
                {
                    Language = value.Value;
                    this.RaisePropertyChanged(nameof(SelectedLanguage));
                }
            }
        }

        public SettingOption SelectedTheme
        {
            get => ThemeOptions.FirstOrDefault(option => option.Value == Theme) ?? ThemeOptions[0];
            set
            {
                if (value is null)
                {
                    return;
                }

                if (Theme != value.Value)
                {
                    Theme = value.Value;
                    this.RaisePropertyChanged(nameof(SelectedTheme));
                }
            }
        }

        public string Language
        {
            get => _settings.General.Language;
            set
            {
                if (_settings.General.Language != value)
                {
                    _settings.General.Language = value;
                    this.RaisePropertyChanged(nameof(Language));
                    NotifySettingsEdited();
                }
            }
        }

        public string Theme
        {
            get => _settings.General.Theme;
            set
            {
                if (_settings.General.Theme != value)
                {
                    _settings.General.Theme = value;
                    this.RaisePropertyChanged(nameof(Theme));
                    NotifySettingsEdited();
                }
            }
        }

        public bool AutoSave
        {
            get => _settings.General.AutoSave;
            set
            {
                if (_settings.General.AutoSave != value)
                {
                    _settings.General.AutoSave = value;
                    this.RaisePropertyChanged(nameof(AutoSave));
                    NotifySettingsEdited();
                }
            }
        }

        public int AutoSaveInterval
        {
            get => _settings.General.AutoSaveInterval;
            set
            {
                if (_settings.General.AutoSaveInterval != value)
                {
                    _settings.General.AutoSaveInterval = value;
                    this.RaisePropertyChanged(nameof(AutoSaveInterval));
                    NotifySettingsEdited();
                }
            }
        }

        public GeneralSettingsViewModel(EditorSettings settings) : base(settings)
        {
        }

        protected override void OnSettingsUpdated()
        {
            this.RaisePropertyChanged(nameof(Language));
            this.RaisePropertyChanged(nameof(Theme));
            this.RaisePropertyChanged(nameof(AutoSave));
            this.RaisePropertyChanged(nameof(AutoSaveInterval));
            this.RaisePropertyChanged(nameof(SelectedLanguage));
            this.RaisePropertyChanged(nameof(SelectedTheme));
        }

        public sealed record SettingOption(string Value, string Display);
    }
}
