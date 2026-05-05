using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Settings
{
    public class GeneralSettingsViewModel : SettingsCategoryViewModel
    {
        private readonly IFileDialogService? _fileDialogService;

        public override string Name => "総合";

        public IReadOnlyList<SettingOption> LanguageOptions { get; } =
        [
            new SettingOption("ja", "Japanese")
        ];

        public IReadOnlyList<SettingOption> ThemeOptions { get; } =
        [
            new SettingOption("auto", "Auto"),
            new SettingOption("dark", "Dark"),
            new SettingOption("light", "Light")
        ];

        public IReadOnlyList<SettingOption> MediaPathStyleOptions { get; } =
        [
            new SettingOption("Relative", "Relative (from project)"),
            new SettingOption("Absolute", "Absolute")
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

        public SettingOption SelectedMediaPathStyle
        {
            get => MediaPathStyleOptions.FirstOrDefault(option => option.Value == _settings.General.MediaPathStyle.ToString()) ?? MediaPathStyleOptions[0];
            set
            {
                if (value is null)
                {
                    return;
                }

                if (_settings.General.MediaPathStyle.ToString() != value.Value)
                {
                    _settings.General.MediaPathStyle = Enum.Parse<MediaPathStyle>(value.Value);
                    this.RaisePropertyChanged(nameof(SelectedMediaPathStyle));
                    NotifySettingsEdited();
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

        public bool AutoBackup
        {
            get => _settings.General.AutoBackup;
            set
            {
                if (_settings.General.AutoBackup != value)
                {
                    _settings.General.AutoBackup = value;
                    this.RaisePropertyChanged(nameof(AutoBackup));
                    NotifySettingsEdited();
                }
            }
        }

        public int AutoBackupInterval
        {
            get => _settings.General.AutoBackupInterval;
            set
            {
                if (_settings.General.AutoBackupInterval != value)
                {
                    _settings.General.AutoBackupInterval = value;
                    this.RaisePropertyChanged(nameof(AutoBackupInterval));
                    NotifySettingsEdited();
                }
            }
        }

        public string AutoBackupPath
        {
            get => _settings.General.AutoBackupPath;
            set
            {
                if (_settings.General.AutoBackupPath != value)
                {
                    _settings.General.AutoBackupPath = value;
                    this.RaisePropertyChanged(nameof(AutoBackupPath));
                    this.RaisePropertyChanged(nameof(AutoBackupPathDisplay));
                    NotifySettingsEdited();
                }
            }
        }

        public string AutoBackupPathDisplay => string.IsNullOrEmpty(AutoBackupPath)
            ? "(Project folder/backup)"
            : AutoBackupPath;

        public int AutoBackupMaxCount
        {
            get => _settings.General.AutoBackupMaxCount;
            set
            {
                if (_settings.General.AutoBackupMaxCount != value)
                {
                    _settings.General.AutoBackupMaxCount = value;
                    this.RaisePropertyChanged(nameof(AutoBackupMaxCount));
                    NotifySettingsEdited();
                }
            }
        }

        public ReactiveCommand<Unit, Unit> SelectBackupPathCommand { get; }

        public GeneralSettingsViewModel(EditorSettings settings) : base(settings)
        {
            SelectBackupPathCommand = ReactiveCommand.CreateFromTask(SelectBackupPathExecute);
        }

        public GeneralSettingsViewModel(EditorSettings settings, IFileDialogService fileDialogService) : base(settings)
        {
            _fileDialogService = fileDialogService;
            SelectBackupPathCommand = ReactiveCommand.CreateFromTask(SelectBackupPathExecute);
        }

        private async Task SelectBackupPathExecute()
        {
            if (_fileDialogService is null) return;

            var folder = await _fileDialogService.OpenFolderDialogAsync();
            if (folder is null) return;

            var path = folder.Path?.LocalPath;
            if (!string.IsNullOrEmpty(path))
            {
                AutoBackupPath = path;
            }
        }

        protected override void OnSettingsUpdated()
        {
            this.RaisePropertyChanged(nameof(Language));
            this.RaisePropertyChanged(nameof(Theme));
            this.RaisePropertyChanged(nameof(AutoSave));
            this.RaisePropertyChanged(nameof(AutoSaveInterval));
            this.RaisePropertyChanged(nameof(AutoBackup));
            this.RaisePropertyChanged(nameof(AutoBackupInterval));
            this.RaisePropertyChanged(nameof(AutoBackupPath));
            this.RaisePropertyChanged(nameof(AutoBackupPathDisplay));
            this.RaisePropertyChanged(nameof(AutoBackupMaxCount));
            this.RaisePropertyChanged(nameof(SelectedLanguage));
            this.RaisePropertyChanged(nameof(SelectedTheme));
            this.RaisePropertyChanged(nameof(SelectedMediaPathStyle));
        }

        public sealed record SettingOption(string Value, string Display);
    }
}
