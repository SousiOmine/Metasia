using Metasia.Editor.Models;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.Views.Inspector.Properties;
using ReactiveUI;
using Metasia.Core.Coordinate;
using System;
using Metasia.Editor.Models.States;
using Metasia.Core.Media;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;

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
    public bool IsMediaPathProperty
    {
        get => _isMediaPathProperty;
        set => this.RaiseAndSetIfChanged(ref _isMediaPathProperty, value);
    }
    public MediaPathPropertyViewModel? MediaPathPropertyVm
    {
        get => _mediaPathPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _mediaPathPropertyVm, value);
    }
    public bool IsStringProperty
    {
        get => _isStringProperty;
        set => this.RaiseAndSetIfChanged(ref _isStringProperty, value);
    }
    public StringPropertyViewModel? StringPropertyVm
    {
        get => _stringPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _stringPropertyVm, value);
    }
    public bool IsFontProperty
    {
        get => _isFontProperty;
        set => this.RaiseAndSetIfChanged(ref _isFontProperty, value);
    }
    public MetaFontParamPropertyViewModel? FontPropertyVm
    {
        get => _metaFontParamPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _metaFontParamPropertyVm, value);
    }
    public bool IsDoubleProperty
    {
        get => _isDoubleProperty;
        set => this.RaiseAndSetIfChanged(ref _isDoubleProperty, value);
    }
    public DoublePropertyViewModel? DoublePropertyVm
    {
        get => _doublePropertyVm;
        set => this.RaiseAndSetIfChanged(ref _doublePropertyVm, value);
    }
    public bool IsMetaEnumParamProperty
    {
        get => _isMetaEnumParamProperty;
        set => this.RaiseAndSetIfChanged(ref _isMetaEnumParamProperty, value);
    }
    public MetaEnumParamPropertyViewModel? MetaEnumParamPropertyVm
    {
        get => _metaEnumParamPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _metaEnumParamPropertyVm, value);
    }
    public bool IsColorProperty
    {
        get => _isColorProperty;
        set => this.RaiseAndSetIfChanged(ref _isColorProperty, value);
    }
    public ColorPropertyViewModel? ColorPropertyVm
    {
        get => _colorPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _colorPropertyVm, value);
    }
    public bool IsLayerTargetProperty
    {
        get => _isLayerTargetProperty;
        set => this.RaiseAndSetIfChanged(ref _isLayerTargetProperty, value);
    }
    public LayerTargetPropertyViewModel? LayerTargetPropertyVm
    {
        get => _layerTargetPropertyVm;
        set => this.RaiseAndSetIfChanged(ref _layerTargetPropertyVm, value);
    }
    public string PlaceholderText
    {
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
    private MediaPathPropertyViewModel? _mediaPathPropertyVm;
    private StringPropertyViewModel? _stringPropertyVm;
    private MetaFontParamPropertyViewModel? _metaFontParamPropertyVm;
    private DoublePropertyViewModel? _doublePropertyVm;
    private MetaEnumParamPropertyViewModel? _metaEnumParamPropertyVm;
    private ColorPropertyViewModel? _colorPropertyVm;
    private LayerTargetPropertyViewModel? _layerTargetPropertyVm;
    private bool _isMetaNumberParamProperty = false;
    private bool _isMediaPathProperty = false;
    private bool _isStringProperty = false;
    private bool _isFontProperty = false;
    private bool _isDoubleProperty = false;
    private bool _isMetaEnumParamProperty = false;
    private bool _isColorProperty = false;
    private bool _isLayerTargetProperty = false;
    private bool _usePlaceholder;
    private ObjectPropertyFinder.EditablePropertyInfo _propertyInfo;
    private readonly IMetaNumberParamPropertyViewModelFactory _metaNumberParamPropertyViewModelFactory;
    private readonly IMediaPathPropertyViewModelFactory _mediaPathPropertyViewModelFactory;
    private readonly IStringPropertyViewModelFactory _stringPropertyViewModelFactory;
    private readonly IDoublePropertyViewModelFactory _doublePropertyViewModelFactory;
    private readonly IMetaEnumParamPropertyViewModelFactory _metaEnumParamPropertyViewModelFactory;
    private readonly IMetaFontParamPropertyViewModelFactory _metaFontParamPropertyViewModelFactory;
    private readonly IColorPropertyViewModelFactory _colorPropertyViewModelFactory;
    private readonly ILayerTargetPropertyViewModelFactory _layerTargetPropertyViewModelFactory;
    private readonly IProjectState _projectState;
    public PropertyRouterViewModel(
        ObjectPropertyFinder.EditablePropertyInfo propertyInfo,
        IMetaNumberParamPropertyViewModelFactory metaNumberParamPropertyViewModelFactory,
        IMediaPathPropertyViewModelFactory mediaPathPropertyViewModelFactory,
        IStringPropertyViewModelFactory stringPropertyViewModelFactory,
        IDoublePropertyViewModelFactory doublePropertyViewModelFactory,
        IMetaEnumParamPropertyViewModelFactory metaEnumParamPropertyViewModelFactory,
        IMetaFontParamPropertyViewModelFactory metaFontParamPropertyViewModelFactory,
        IColorPropertyViewModelFactory colorPropertyViewModelFactory,
        ILayerTargetPropertyViewModelFactory layerTargetPropertyViewModelFactory,
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        ArgumentNullException.ThrowIfNull(metaNumberParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(mediaPathPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(stringPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(doublePropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(metaEnumParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(metaFontParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(colorPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(layerTargetPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(projectState);
        _metaNumberParamPropertyViewModelFactory = metaNumberParamPropertyViewModelFactory;
        _mediaPathPropertyViewModelFactory = mediaPathPropertyViewModelFactory;
        _stringPropertyViewModelFactory = stringPropertyViewModelFactory;
        _doublePropertyViewModelFactory = doublePropertyViewModelFactory;
        _metaEnumParamPropertyViewModelFactory = metaEnumParamPropertyViewModelFactory;
        _metaFontParamPropertyViewModelFactory = metaFontParamPropertyViewModelFactory;
        _colorPropertyViewModelFactory = colorPropertyViewModelFactory;
        _layerTargetPropertyViewModelFactory = layerTargetPropertyViewModelFactory;
        _projectState = projectState;
        _propertyInfo = propertyInfo;
        _projectState.TimelineChanged += OnTimelineChanged;


        RestructureProperty();

    }

    private void RestructureProperty()
    {
        if (_propertyInfo.Type == typeof(MetaNumberParam<double>))
        {
            if (MetaNumberParamPropertyVm is null)
            {
                if (_propertyInfo.Min is null || _propertyInfo.Max is null || _propertyInfo.RecommendedMin is null || _propertyInfo.RecommendedMax is null)
                {
                    MetaNumberParamPropertyVm = _metaNumberParamPropertyViewModelFactory.Create(_propertyInfo.Identifier, (MetaNumberParam<double>)_propertyInfo.PropertyValue!);
                }
                else
                {
                    MetaNumberParamPropertyVm = _metaNumberParamPropertyViewModelFactory.Create(_propertyInfo.Identifier, (MetaNumberParam<double>)_propertyInfo.PropertyValue!, _propertyInfo.Min.Value, _propertyInfo.Max.Value, _propertyInfo.RecommendedMin.Value, _propertyInfo.RecommendedMax.Value);
                }
                IsMetaNumberParamProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(MediaPath))
        {
            if (MediaPathPropertyVm is null)
            {
                MediaPathPropertyVm = _mediaPathPropertyViewModelFactory.Create(_propertyInfo.Identifier, (MediaPath)_propertyInfo.PropertyValue!);
                IsMediaPathProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(string))
        {
            if (StringPropertyVm is null)
            {
                StringPropertyVm = _stringPropertyViewModelFactory.Create(_propertyInfo.Identifier, (string)_propertyInfo.PropertyValue!);
                IsStringProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(MetaFontParam))
        {
            if (FontPropertyVm is null)
            {
                FontPropertyVm = _metaFontParamPropertyViewModelFactory.Create(_propertyInfo.Identifier, (MetaFontParam)_propertyInfo.PropertyValue!);
                IsFontProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(MetaDoubleParam))
        {
            if (DoublePropertyVm is null)
            {
                var min = _propertyInfo.Min ?? double.MinValue;
                var max = _propertyInfo.Max ?? double.MaxValue;
                var recommendMin = _propertyInfo.RecommendedMin ?? min;
                var recommendMax = _propertyInfo.RecommendedMax ?? max;
                var metaDoubleParam = (MetaDoubleParam)_propertyInfo.PropertyValue!;
                DoublePropertyVm = _doublePropertyViewModelFactory.Create(_propertyInfo.Identifier, metaDoubleParam.Value, min, max, recommendMin, recommendMax);
                IsDoubleProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(MetaEnumParam))
        {
            if (MetaEnumParamPropertyVm is null)
            {
                MetaEnumParamPropertyVm = _metaEnumParamPropertyViewModelFactory.Create(_propertyInfo.Identifier, (MetaEnumParam)_propertyInfo.PropertyValue!);
                IsMetaEnumParamProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(ColorRgb8))
        {
            if (ColorPropertyVm is null)
            {
                ColorPropertyVm = _colorPropertyViewModelFactory.Create(_propertyInfo.Identifier, (ColorRgb8)_propertyInfo.PropertyValue!);
                IsColorProperty = true;
                UsePlaceholder = false;
            }
        }
        else if (_propertyInfo.Type == typeof(LayerTarget))
        {
            if (LayerTargetPropertyVm is null)
            {
                LayerTargetPropertyVm = _layerTargetPropertyViewModelFactory.Create(_propertyInfo.Identifier, (LayerTarget)_propertyInfo.PropertyValue!);
                IsLayerTargetProperty = true;
                UsePlaceholder = false;
            }
        }
        else
        {
            PlaceholderText = _propertyInfo.Identifier + " " + _propertyInfo.Type;
            UsePlaceholder = true;
        }
    }

    private void OnTimelineChanged()
    {
        RestructureProperty();
    }

}
