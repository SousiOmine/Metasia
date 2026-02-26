using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;
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

    public VisualEffectsViewModel(
        IRenderable target,
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
            var selectVm = new NewObjectSelectViewModel(NewObjectSelectViewModel.TargetType.VisualEffect);
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

        foreach (VisualEffectBase effect in _target.VisualEffects)
        {
            VisualEffectItems.Add(new VisualEffectItemViewModel(effect));
        }

        if (VisualEffectItems.Any(x => x.EffectId == selectedId))
        {
            var selectedItem = VisualEffectItems.First(x => x.EffectId == selectedId);
            SelectedVisualEffectItem = selectedItem;
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
            Properties.Add(_propertyRouterViewModelFactory.Create(property));
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

    public VisualEffectItemViewModel(IVisualEffect effect)
    {
        EffectId = effect.Id;
        EffectName = effect.GetType().Name;
    }
}
