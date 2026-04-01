using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Sounds;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Dialogs;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector;

public class AudioEffectsViewModel : ViewModelBase
{
    public ObservableCollection<AudioEffectItemViewModel> AudioEffectItems { get; } = new();

    public AudioEffectItemViewModel? SelectedAudioEffectItem
    {
        get => _selectedAudioEffectItem;
        set
        {
            if (_selectedAudioEffectItem == value) return;
            this.RaiseAndSetIfChanged(ref _selectedAudioEffectItem, value);
            LoadProperties();
        }
    }

    public ObservableCollection<PropertyRouterViewModel> Properties { get; set; } = new();

    public ICommand NewEffectCommand { get; init; }
    public ICommand DeleteEffectCommand { get; init; }
    public Interaction<NewObjectSelectViewModel, IMetasiaObject?> NewObjectSelectInteraction { get; } = new();

    private AudioEffectItemViewModel? _selectedAudioEffectItem;
    private readonly IAudible _target;
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;

    public AudioEffectsViewModel(
        IAudible target,
        IProjectState projectState,
        IEditCommandManager editCommandManager,
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory)
    {
        _target = target;
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;

        NewEffectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var selectVm = new NewObjectSelectViewModel(NewObjectSelectViewModel.TargetType.AudioEffect);
            var result = await NewObjectSelectInteraction.Handle(selectVm);
            if (result is AudioEffectBase effect)
            {
                var command = new AudioEffectAddCommand(_target, effect);
                _editCommandManager.Execute(command);
            }
        });

        DeleteEffectCommand = ReactiveCommand.Create(TryDeleteEffect);

        _projectState.TimelineChanged += LoadEffects;

        LoadEffects();
    }

    private void LoadEffects()
    {
        var selectedId = SelectedAudioEffectItem is null ? string.Empty : SelectedAudioEffectItem.EffectId;
        AudioEffectItems.Clear();

        for (int i = 0; i < _target.AudioEffects.Count; i++)
        {
            var effect = _target.AudioEffects[i];
            AudioEffectItems.Add(new AudioEffectItemViewModel(
                effect,
                canMoveUp: i > 0,
                canMoveDown: i < _target.AudioEffects.Count - 1,
                moveUp: () => TryMoveEffect(effect.Id, -1),
                moveDown: () => TryMoveEffect(effect.Id, 1),
                changeIsActive: isActive => TryChangeEffectIsActive(effect.Id, isActive)));
        }

        if (AudioEffectItems.Any(x => x.EffectId == selectedId))
        {
            SelectedAudioEffectItem = AudioEffectItems.First(x => x.EffectId == selectedId);
        }

        LoadProperties();
    }

    private void LoadProperties()
    {
        Properties.Clear();
        var selectedEffect = _target.AudioEffects.FirstOrDefault(x => x.Id == SelectedAudioEffectItem?.EffectId);
        if (selectedEffect is null) return;

        var editableProperties = ObjectPropertyFinder.FindEditableProperties(selectedEffect);
        foreach (var property in editableProperties)
        {
            Properties.Add(_propertyRouterViewModelFactory.Create(property));
        }
    }

    private void TryDeleteEffect()
    {
        if (SelectedAudioEffectItem is null) return;
        var selectedEffect = _target.AudioEffects.Find(x => x.Id == SelectedAudioEffectItem.EffectId);
        if (selectedEffect is null) return;

        var command = new AudioEffectRemoveCommand(_target, selectedEffect);
        _editCommandManager.Execute(command);
    }

    private void TryMoveEffect(string effectId, int delta)
    {
        var effect = _target.AudioEffects.FirstOrDefault(x => x.Id == effectId);
        if (effect is null) return;

        int currentIndex = _target.AudioEffects.IndexOf(effect);
        if (currentIndex == -1) return;

        int newIndex = currentIndex + delta;
        if (newIndex < 0 || newIndex >= _target.AudioEffects.Count) return;

        var command = new AudioEffectMoveCommand(_target, effect, newIndex);
        _editCommandManager.Execute(command);
    }

    private void TryChangeEffectIsActive(string effectId, bool isActive)
    {
        var effect = _target.AudioEffects.FirstOrDefault(x => x.Id == effectId);
        if (effect is null || effect.IsActive == isActive) return;

        var command = new AudioEffectIsActiveChangeCommand(_target, effect, isActive);
        _editCommandManager.Execute(command);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projectState.TimelineChanged -= LoadEffects;
        }

        base.Dispose(disposing);
    }
}

public sealed class AudioEffectItemViewModel : ViewModelBase
{
    public string EffectId { get; init; } = string.Empty;
    public string EffectName { get; init; } = string.Empty;
    public bool CanMoveUp { get; }
    public bool CanMoveDown { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            this.RaiseAndSetIfChanged(ref _isActive, value);
            _changeIsActive(value);
        }
    }

    private readonly Action<bool> _changeIsActive;
    private bool _isActive;

    public AudioEffectItemViewModel(
        IAudioEffect effect,
        bool canMoveUp,
        bool canMoveDown,
        Action moveUp,
        Action moveDown,
        Action<bool> changeIsActive)
    {
        EffectId = effect.Id;
        EffectName = DisplayTextResolver.ResolveAudioEffectDisplayName(effect.GetType());
        CanMoveUp = canMoveUp;
        CanMoveDown = canMoveDown;
        _isActive = effect.IsActive;
        _changeIsActive = changeIsActive ?? throw new ArgumentNullException(nameof(changeIsActive));
        MoveUpCommand = ReactiveCommand.Create(() => moveUp());
        MoveDownCommand = ReactiveCommand.Create(() => moveDown());
    }
}
