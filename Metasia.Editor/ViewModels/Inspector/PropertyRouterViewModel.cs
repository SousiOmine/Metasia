using Metasia.Editor.Models;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.Views.Inspector.Properties;
using ReactiveUI;
using Metasia.Core.Coordinate;

namespace Metasia.Editor.ViewModels.Inspector;

public class PropertyRouterViewModel : ViewModelBase
{
    public bool IsMetaNumberParamProperty
    {
        get => _isMetaNumberParamProperty;
        set => this.RaiseAndSetIfChanged(ref _isMetaNumberParamProperty, value);
    }
    public MetaNumberParamPropertyViewModel? MetaNumberParamPropertyVm
    {
        get => _metaNumberParamPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _metaNumberParamPropertyVm, value);
    }
    public string PlaceholderText { 
        get => _placeholderText;
        set => this.RaiseAndSetIfChanged(ref _placeholderText, value);
    }

    public bool UsePlaceholder
    {
        get => _usePlaceholder;
        set => this.RaiseAndSetIfChanged(ref _usePlaceholder, value);
    }

    private string _placeholderText = string.Empty;
    private MetaNumberParamPropertyViewModel? _metaNumberParamPropertyVm;
    private bool _isMetaNumberParamProperty;
    private bool _usePlaceholder;
    public PropertyRouterViewModel(ObjectPropertyFinder.EditablePropertyInfo propertyInfo)
    {
        if (propertyInfo.Type == typeof(MetaNumberParam<double>))
        {
            MetaNumberParamPropertyVm = new MetaNumberParamPropertyViewModel(propertyInfo.Identifier, (MetaNumberParam<double>)propertyInfo.PropertyValue!);
            IsMetaNumberParamProperty = true;
            UsePlaceholder = false;
        }
        else
        {
            PlaceholderText = propertyInfo.Identifier + " " + propertyInfo.Type;
            UsePlaceholder = true;
        }
    }
    
}