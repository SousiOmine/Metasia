using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Metasia.Editor.Models.Media;
using ReactiveUI;
using Metasia.Editor.Models.Settings;

namespace Metasia.Editor.ViewModels.Settings
{
    public class EditorSettingsViewModel : SettingsCategoryViewModel
    {
        private readonly MediaAccessorRouter _mediaAccessorRouter;

        public override string Name => "Editor";

        public ObservableCollection<MediaAccessorPriorityItemViewModel> MediaAccessorPriority { get; } = [];

        private MediaAccessorPriorityItemViewModel? _selectedMediaAccessorPriority;
        public MediaAccessorPriorityItemViewModel? SelectedMediaAccessorPriority
        {
            get => _selectedMediaAccessorPriority;
            set => this.RaiseAndSetIfChanged(ref _selectedMediaAccessorPriority, value);
        }

        public ReactiveCommand<Unit, Unit> MovePriorityUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MovePriorityDownCommand { get; }

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
        public EditorSettingsViewModel(EditorSettings settings, MediaAccessorRouter mediaAccessorRouter) : base(settings)
        {
            _mediaAccessorRouter = mediaAccessorRouter ?? throw new ArgumentNullException(nameof(mediaAccessorRouter));
            RebuildPriorityItems();

            var collectionChanged = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => MediaAccessorPriority.CollectionChanged += h,
                h => MediaAccessorPriority.CollectionChanged -= h);

            var canMoveUp = this.WhenAnyValue(x => x.SelectedMediaAccessorPriority)
                .Merge(collectionChanged.Select(_ => SelectedMediaAccessorPriority))
                .Select(_ => CanMoveUp());
            var canMoveDown = this.WhenAnyValue(x => x.SelectedMediaAccessorPriority)
                .Merge(collectionChanged.Select(_ => SelectedMediaAccessorPriority))
                .Select(_ => CanMoveDown());

            MovePriorityUpCommand = ReactiveCommand.Create(MovePriorityUp, canMoveUp);
            MovePriorityDownCommand = ReactiveCommand.Create(MovePriorityDown, canMoveDown);
        }

        protected override void OnSettingsUpdated()
        {
            this.RaisePropertyChanged(nameof(SnapToGrid));
            RebuildPriorityItems();
        }

        private void MovePriorityUp()
        {
            if (SelectedMediaAccessorPriority is null)
            {
                return;
            }

            var index = MediaAccessorPriority.IndexOf(SelectedMediaAccessorPriority);
            if (index <= 0)
            {
                return;
            }

            MediaAccessorPriority.Move(index, index - 1);
            UpdatePriorityOrderSetting();
            this.RaisePropertyChanged(nameof(SelectedMediaAccessorPriority));
        }

        private void MovePriorityDown()
        {
            if (SelectedMediaAccessorPriority is null)
            {
                return;
            }

            var index = MediaAccessorPriority.IndexOf(SelectedMediaAccessorPriority);
            if (index < 0 || index >= MediaAccessorPriority.Count - 1)
            {
                return;
            }

            MediaAccessorPriority.Move(index, index + 1);
            UpdatePriorityOrderSetting();
            this.RaisePropertyChanged(nameof(SelectedMediaAccessorPriority));
        }

        private bool CanMoveUp()
        {
            if (SelectedMediaAccessorPriority is null)
            {
                return false;
            }

            return MediaAccessorPriority.IndexOf(SelectedMediaAccessorPriority) > 0;
        }

        private bool CanMoveDown()
        {
            if (SelectedMediaAccessorPriority is null)
            {
                return false;
            }

            var index = MediaAccessorPriority.IndexOf(SelectedMediaAccessorPriority);
            return index >= 0 && index < MediaAccessorPriority.Count - 1;
        }

        private void RebuildPriorityItems()
        {
            var currentSelectedId = SelectedMediaAccessorPriority?.Id;
            MediaAccessorPriority.Clear();

            var registeredInfos = _mediaAccessorRouter.GetRegisteredAccessorInfos();
            var infoMap = registeredInfos.ToDictionary(x => x.Id, x => x, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            var orderedIds = _settings.Editor.MediaAccessorPriorityOrder
                .Where(id => !string.IsNullOrWhiteSpace(id) && seen.Add(id))
                .Where(id => infoMap.ContainsKey(id))
                .ToList();

            foreach (var id in orderedIds)
            {
                var info = infoMap[id];
                MediaAccessorPriority.Add(new MediaAccessorPriorityItemViewModel(info.Id, info.DisplayName));
            }

            foreach (var info in registeredInfos)
            {
                if (seen.Add(info.Id))
                {
                    MediaAccessorPriority.Add(new MediaAccessorPriorityItemViewModel(info.Id, info.DisplayName));
                }
            }

            SelectedMediaAccessorPriority = MediaAccessorPriority.FirstOrDefault(x => x.Id == currentSelectedId)
                ?? MediaAccessorPriority.FirstOrDefault();
        }

        private void UpdatePriorityOrderSetting()
        {
            _settings.Editor.MediaAccessorPriorityOrder = MediaAccessorPriority.Select(x => x.Id).ToList();
            NotifySettingsEdited();
        }
    }

    public sealed class MediaAccessorPriorityItemViewModel
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string DisplayLabel => $"{DisplayName} ({Id})";

        public MediaAccessorPriorityItemViewModel(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}
