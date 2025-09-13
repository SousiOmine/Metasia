using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector;

public class ClipSettingPaneViewModel : ViewModelBase
{
    public ClipObject? TargetObject { 
        get => _targetObject;
        set {
            if (_targetObject == value) return;
            _targetObject = value;
            TargetObjectChanged?.Invoke(this, EventArgs.Empty);
            buildSettingUI();
        } 
    }
    
    public bool IsActiveCheck
    {
        get => _isActiveCheck;
        set => this.RaiseAndSetIfChanged(ref _isActiveCheck, value);
    }

    public ObservableCollection<PropertyRouterViewModel> Properties { get; set; } = new();
    
    public ICommand IsActiveCheckCommand { get; set; }

    public event EventHandler? TargetObjectChanged;

    private ClipObject? _targetObject;
    private bool _isActiveCheck;
    private readonly IPropertyRouterViewModelFactory _propertyRouterViewModelFactory;
    private readonly IEditCommandManager _editCommandManager;
    public ClipSettingPaneViewModel(
        IPropertyRouterViewModelFactory propertyRouterViewModelFactory,
        IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(propertyRouterViewModelFactory);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        IsActiveCheckCommand = ReactiveCommand.Create(() => { isActiveCheck_Click(); });
        _propertyRouterViewModelFactory = propertyRouterViewModelFactory;
        _editCommandManager = editCommandManager;
    }

    private void buildSettingUI()
    {
        if (TargetObject is null) return;
        
        IsActiveCheck = TargetObject.IsActive;

        var editableProperties = ObjectPropertyFinder.FindEditableProperties(TargetObject);

        Properties.Clear();
        foreach (var property in editableProperties)
        {
            Properties.Add(_propertyRouterViewModelFactory.Create(property));
        }
    }
    
    private void isActiveCheck_Click()
    {
        if (TargetObject is null) return;
        
        var command = new ClipsIsActiveChangeCommand([TargetObject], IsActiveCheck);
        _editCommandManager.Execute(command);
    }
    
    
}
