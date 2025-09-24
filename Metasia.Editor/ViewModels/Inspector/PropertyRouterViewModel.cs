﻿using Metasia.Editor.Models;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Inspector.Properties;
using Metasia.Editor.Views.Inspector.Properties;
using ReactiveUI;
using Metasia.Core.Coordinate;
using System;
using Metasia.Editor.Models.States;
using Metasia.Core.Media;

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
    private MediaPathPropertyViewModel? _mediaPathPropertyVm;
    private bool _isMetaNumberParamProperty = false;
    private bool _isMediaPathProperty = false;
    private bool _usePlaceholder;
    private ObjectPropertyFinder.EditablePropertyInfo _propertyInfo;
    private readonly IMetaNumberParamPropertyViewModelFactory _metaNumberParamPropertyViewModelFactory;
    private readonly IMediaPathPropertyViewModelFactory _mediaPathPropertyViewModelFactory;
    private readonly IProjectState _projectState;
    public PropertyRouterViewModel(
        ObjectPropertyFinder.EditablePropertyInfo propertyInfo, 
        IMetaNumberParamPropertyViewModelFactory metaNumberParamPropertyViewModelFactory,
        IMediaPathPropertyViewModelFactory mediaPathPropertyViewModelFactory,
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(metaNumberParamPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(mediaPathPropertyViewModelFactory);
        ArgumentNullException.ThrowIfNull(projectState);
        _metaNumberParamPropertyViewModelFactory = metaNumberParamPropertyViewModelFactory;
        _mediaPathPropertyViewModelFactory = mediaPathPropertyViewModelFactory;
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