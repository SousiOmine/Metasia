using System.Collections.ObjectModel;
using System.Diagnostics;
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
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory
        )
    {
        _target = target;
        _projectState = projectState;
        _editCommandManager = editCommandManager;
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;

        NewEffectCommand = ReactiveCommand.CreateFromTask(async() =>
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
        
        foreach (AudioEffectBase effect in _target.AudioEffects)
        {
            AudioEffectItems.Add(new AudioEffectItemViewModel(effect));
        }

        if (AudioEffectItems.Any(x => x.EffectId == selectedId))
        {
            var selectedItem = AudioEffectItems.First(x => x.EffectId == selectedId);
            SelectedAudioEffectItem = selectedItem;
        }
        
        LoadProperties();
    }

    private void LoadProperties()
    {
        var selectedEffect = _target.AudioEffects.FirstOrDefault(x => x.Id == SelectedAudioEffectItem?.EffectId);
        if (selectedEffect is null) return;
        var editableProperties = ObjectPropertyFinder.FindEditableProperties(selectedEffect);

        Properties.Clear();
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

    public AudioEffectItemViewModel(IAudioEffect effect)
    {
        EffectId = effect.Id;
        EffectName = effect.GetType().Name;
    }
}