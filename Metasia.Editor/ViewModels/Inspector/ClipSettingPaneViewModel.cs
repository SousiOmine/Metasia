using System;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
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

    public string DebugTest
    {
        get => _debugTest;
        set => this.RaiseAndSetIfChanged(ref _debugTest, value);
    }

    public event EventHandler? TargetObjectChanged;

    private ClipObject? _targetObject;
    private string _debugTest = string.Empty;

    public ClipSettingPaneViewModel()
    {
    }

    private void buildSettingUI()
    {
        if (TargetObject is null) return;

        var editableProperties = ObjectPropertyFinder.FindEditableProperties(TargetObject);
        DebugTest = string.Join(", \n", editableProperties.Select(x => x.Identifier));
    }
}
