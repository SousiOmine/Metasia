using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Metasia.Editor.Models.Media;
using ReactiveUI;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.ViewModels.Settings;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly MediaAccessorRouter _mediaAccessorRouter;
        private EditorSettings _workingSettings;
        private EditorSettings _lastAppliedSettings;

        public ObservableCollection<SettingsCategoryViewModel> Categories { get; } = new();

        private SettingsCategoryViewModel _selectedCategory;
        public SettingsCategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
        }

        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetToDefaultsCommand { get; }

        public SettingsWindowViewModel(ISettingsService settingsService, MediaAccessorRouter mediaAccessorRouter)
        {
            _settingsService = settingsService;
            _mediaAccessorRouter = mediaAccessorRouter;
            _workingSettings = CloneSettings(_settingsService.CurrentSettings);
            _lastAppliedSettings = CloneSettings(_settingsService.CurrentSettings);

            Categories.Add(new GeneralSettingsViewModel(_workingSettings));
            Categories.Add(new EditorSettingsViewModel(_workingSettings, _mediaAccessorRouter));

            foreach (var category in Categories)
            {
                category.SettingsEdited += OnSettingsEdited;
            }

            var canApplyOrCancel = this.WhenAnyValue(vm => vm.HasChanges);
            ApplyCommand = ReactiveCommand.CreateFromTask(ApplyAsync, canApplyOrCancel);
            CancelCommand = ReactiveCommand.Create(CancelChanges, canApplyOrCancel);
            ResetToDefaultsCommand = ReactiveCommand.Create(ResetToDefaults);

            SelectedCategory = Categories.FirstOrDefault();
        }

        private void OnSettingsEdited()
        {
            HasChanges = true;
        }

        private async Task ApplyAsync()
        {
            await _settingsService.UpdateSettingsAsync(CloneSettings(_workingSettings));
            _lastAppliedSettings = CloneSettings(_workingSettings);
            HasChanges = false;
        }

        private void CancelChanges()
        {
            SetWorkingSettings(CloneSettings(_lastAppliedSettings));
            HasChanges = false;
        }

        private void ResetToDefaults()
        {
            SetWorkingSettings(new EditorSettings());
            HasChanges = true;
        }

        private void SetWorkingSettings(EditorSettings settings)
        {
            _workingSettings = settings;
            foreach (var category in Categories)
            {
                category.UpdateSettings(_workingSettings);
            }
        }

        private static EditorSettings CloneSettings(EditorSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            return JsonSerializer.Deserialize<EditorSettings>(json) ?? new EditorSettings();
        }
    }
}
