using System;
using Metasia.Core.Coordinate;
using Metasia.Editor.Models;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaNumberParamPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string PropertyValue
    {
        get => _propertyValue;
        set => this.RaiseAndSetIfChanged(ref _propertyValue, value);
    }
    private string _propertyDisplayName = string.Empty;
    private string _propertyValue = string.Empty;

    public MetaNumberParamPropertyViewModel(string propertyIdentifier, MetaNumberParam<double> target)
    {
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = "100(仮)";
    }
    
}