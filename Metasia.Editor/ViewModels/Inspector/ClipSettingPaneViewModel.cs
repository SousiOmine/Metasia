using System;
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
    
    public ICommand IsActiveCheckCommand { get; set; }
    
    public string DebugTest
    {
        get => _debugTest;
        set => this.RaiseAndSetIfChanged(ref _debugTest, value);
    }

    public event EventHandler? TargetObjectChanged;

    private InspectorViewModel _inspectorViewModel;
    private ClipObject? _targetObject;
    private bool _isActiveCheck;
    private string _debugTest = string.Empty;

    public ClipSettingPaneViewModel(InspectorViewModel inspectorViewModel)
    {
        IsActiveCheckCommand = ReactiveCommand.Create(() => { isActiveCheck_Click(); });
        _inspectorViewModel = inspectorViewModel;
    }

    private void buildSettingUI()
    {
        if (TargetObject is null) return;
        
        IsActiveCheck = TargetObject.IsActive;

        var editableProperties = ObjectPropertyFinder.FindEditableProperties(TargetObject);
        DebugTest = string.Join(", \n", editableProperties.Select(x => x.Identifier));
    }
    
    private void isActiveCheck_Click()
    {
        if (TargetObject is null) return;
        
        var command = new ClipsIsActiveChangeCommand([TargetObject], IsActiveCheck);
        _inspectorViewModel.RunEditCommand(command);
    }
    
    
}
