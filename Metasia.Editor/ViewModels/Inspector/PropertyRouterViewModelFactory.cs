using System;
using Metasia.Editor.Models;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public class PropertyRouterViewModelFactory : IPropertyRouterViewModelFactory
{
    private readonly IMetaNumberParamPropertyViewModelFactory _metaNumberParamPropertyViewModelFactory;
    private readonly IMediaPathPropertyViewModelFactory _mediaPathPropertyViewModelFactory;
    private readonly IStringPropertyViewModelFactory _stringPropertyViewModelFactory;
    private readonly IDoublePropertyViewModelFactory _doublePropertyViewModelFactory;
    private readonly IMetaEnumParamPropertyViewModelFactory _metaEnumParamPropertyViewModelFactory;
    private readonly IProjectState _projectState;
    private readonly IMetaFontParamPropertyViewModelFactory _metaFontParamPropertyViewModelFactory;
    private readonly IColorPropertyViewModelFactory _colorPropertyViewModelFactory;
    private readonly ILayerTargetPropertyViewModelFactory _layerTargetPropertyViewModelFactory;

    public PropertyRouterViewModelFactory(
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
    }

    public PropertyRouterViewModel Create(ObjectPropertyFinder.EditablePropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        return new PropertyRouterViewModel(propertyInfo, _metaNumberParamPropertyViewModelFactory, _mediaPathPropertyViewModelFactory, _stringPropertyViewModelFactory, _doublePropertyViewModelFactory, _metaEnumParamPropertyViewModelFactory, _metaFontParamPropertyViewModelFactory, _colorPropertyViewModelFactory, _layerTargetPropertyViewModelFactory, _projectState);
    }
}
