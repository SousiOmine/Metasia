using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Editor.Models;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.ViewModels.Dialogs;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector;

public class VisualEffectsViewModel : ViewModelBase
{
    public ObservableCollection<VisualEffectItemViewModel> VisualEffectItems { get; } = new();

    public VisualEffectItemViewModel? SelectedVisualEffectItem
    {
        get => _selectedVisualEffectItem;
        set
        {
            if (_selectedVisualEffectItem == value) return;
            this.RaiseAndSetIfChanged(ref _selectedVisualEffectItem, value);
            LoadProperties();
        }
    }

    public ObservableCollection<PropertyRouterViewModel> Properties { get; set; } = new();

    public ICommand NewEffectCommand { get; init; }
    public ICommand DeleteEffectCommand { get; init; }
    public Interaction<NewObjectSelectViewModel, IMetasiaObject?> NewObjectSelectInteraction { get; } = new();

    private VisualEffectItemViewModel? _selectedVisualEffectItem;
    private readonly IRenderable _target;
    private readonly IProjectState _projectState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    private readonly INewObjectSelectViewModelFactory _newObjectSelectViewModelFactory;

    public VisualEffectsViewModel(
        IRenderable target,
        IProjectState projectState,
        IEditCommandManager editCommandManager,
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory,
        INewObjectSelectViewModelFactory newObjectSelectViewModelFactory)
    {
        _target = target;
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
        _newObjectSelectViewModelFactory = newObjectSelectViewModelFactory;

        NewEffectCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var selectVm = _newObjectSelectViewModelFactory.Create(NewObjectSelectViewModel.TargetType.VisualEffect);
            var result = await NewObjectSelectInteraction.Handle(selectVm);
            if (result is VisualEffectBase effect)
            {
                var command = new VisualEffectAddCommand(_target, effect);
                _editCommandManager.Execute(command);
            }
        });

        DeleteEffectCommand = ReactiveCommand.Create(TryDeleteEffect);

        _projectState.TimelineChanged += LoadEffects;

        LoadEffects();
    }

    private void LoadEffects()
    {
        var selectedId = SelectedVisualEffectItem is null ? string.Empty : SelectedVisualEffectItem.EffectId;
        VisualEffectItems.Clear();

        for (int i = 0; i < _target.VisualEffects.Count; i++)
        {
            var effect = _target.VisualEffects[i];
            VisualEffectItems.Add(new VisualEffectItemViewModel(
                effect,
                canMoveUp: i > 0,
                canMoveDown: i < _target.VisualEffects.Count - 1,
                moveUp: () => TryMoveEffect(effect.Id, -1),
                moveDown: () => TryMoveEffect(effect.Id, 1),
                changeIsActive: isActive => TryChangeEffectIsActive(effect.Id, isActive)));
        }

        if (VisualEffectItems.Any(x => x.EffectId == selectedId))
        {
            SelectedVisualEffectItem = VisualEffectItems.First(x => x.EffectId == selectedId);
        }
        else
        {
            LoadProperties();
        }
    }

    private void LoadProperties()
    {
        Properties.Clear();
        var selectedEffect = _target.VisualEffects.FirstOrDefault(x => x.Id == SelectedVisualEffectItem?.EffectId);
        if (selectedEffect is null) return;

        var editableProperties = ObjectPropertyFinder.FindEditableProperties(selectedEffect);
        foreach (var property in editableProperties)
        {
            Properties.Add(_propertyRouterViewModelFactory.Create(property, allowMultiClipApply: false));
        }
    }

    private void TryDeleteEffect()
    {
        if (SelectedVisualEffectItem is null) return;
        var selectedEffect = _target.VisualEffects.Find(x => x.Id == SelectedVisualEffectItem.EffectId);
        if (selectedEffect is null) return;

        var command = new VisualEffectRemoveCommand(_target, selectedEffect);
        _editCommandManager.Execute(command);
    }

    private void TryMoveEffect(string effectId, int delta)
    {
        var effect = _target.VisualEffects.FirstOrDefault(x => x.Id == effectId);
        if (effect is null) return;

        int currentIndex = _target.VisualEffects.IndexOf(effect);
        if (currentIndex == -1) return;

        int newIndex = currentIndex + delta;
        if (newIndex < 0 || newIndex >= _target.VisualEffects.Count) return;

        var command = new VisualEffectMoveCommand(_target, effect, newIndex);
        _editCommandManager.Execute(command);
    }

    private void TryChangeEffectIsActive(string effectId, bool isActive)
    {
        var effect = _target.VisualEffects.FirstOrDefault(x => x.Id == effectId);
        if (effect is null || effect.IsActive == isActive) return;

        var command = new VisualEffectIsActiveChangeCommand(_target, effect, isActive);
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

public sealed class VisualEffectItemViewModel : ViewModelBase
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

    public VisualEffectItemViewModel(
        IVisualEffect effect,
        bool canMoveUp,
        bool canMoveDown,
        Action moveUp,
        Action moveDown,
        Action<bool> changeIsActive)
    {
        EffectId = effect.Id;
        EffectName = DisplayTextResolver.ResolveVisualEffectDisplayName(effect.GetType());
        CanMoveUp = canMoveUp;
        CanMoveDown = canMoveDown;
        _isActive = effect.IsActive;
        _changeIsActive = changeIsActive ?? throw new ArgumentNullException(nameof(changeIsActive));
        MoveUpCommand = ReactiveCommand.Create(() => moveUp());
        MoveDownCommand = ReactiveCommand.Create(() => moveDown());
    }
}
