using ReactiveUI;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.ViewModels.Settings
{
    public class EditorSettingsViewModel : SettingsCategoryViewModel
    {
        public override string Name => "Editor";

        public bool SnapToGrid
        {
            get => _settings.Editor.SnapToGrid;
            set
            {
                if (_settings.Editor.SnapToGrid != value)
                {
                    _settings.Editor.SnapToGrid = value;
                    this.RaisePropertyChanged(nameof(SnapToGrid));
                    NotifySettingsEdited();
                }
            }
        }

        public EditorSettingsViewModel(EditorSettings settings) : base(settings)
        {
        }

        protected override void OnSettingsUpdated()
        {
            this.RaisePropertyChanged(nameof(SnapToGrid));
        }
    }
}
