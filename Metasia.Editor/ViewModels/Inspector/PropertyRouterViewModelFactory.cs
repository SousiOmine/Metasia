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
    public PropertyRouterViewModelFactory(IMetaNumberParamPropertyViewModelFactory metaNumberParamPropertyViewModelFactory, IMediaPathPropertyViewModelFactory mediaPathPropertyViewModelFactory, IStringPropertyViewModelFactory stringPropertyViewModelFactory, IDoublePropertyViewModelFactory doublePropertyViewModelFactory, IMetaEnumParamPropertyViewModelFactory metaEnumParamPropertyViewModelFactory, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(metaNumberParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(mediaPathPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(stringPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(doublePropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(metaEnumParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(projectState);
        _metaNumberParamPropertyViewModelFactory = metaNumberParamPropertyViewModelFactory;
        _mediaPathPropertyViewModelFactory = mediaPathPropertyViewModelFactory;
        _stringPropertyViewModelFactory = stringPropertyViewModelFactory;
        _doublePropertyViewModelFactory = doublePropertyViewModelFactory;
        _metaEnumParamPropertyViewModelFactory = metaEnumParamPropertyViewModelFactory;
        _projectState = projectState;
    }

    public PropertyRouterViewModel Create(ObjectPropertyFinder.EditablePropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        return new PropertyRouterViewModel(propertyInfo, _metaNumberParamPropertyViewModelFactory, _mediaPathPropertyViewModelFactory, _stringPropertyViewModelFactory, _doublePropertyViewModelFactory, _metaEnumParamPropertyViewModelFactory, _projectState);
    }
}