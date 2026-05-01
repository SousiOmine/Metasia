using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.ViewModels.Inspector.Properties.Components;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaNumberParamPropertyViewModelFactory : IMetaNumberParamPropertyViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState projectState;
    private readonly IMetaNumberCoordPointViewModelFactory coordPointViewModelFactory;

    public MetaNumberParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        IMetaNumberCoordPointViewModelFactory coordPointViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(coordPointViewModelFactory);
        this.selectionState = selectionState;
        this.editCommandManager = editCommandManager;
        this.projectState = projectState;
        this.coordPointViewModelFactory = coordPointViewModelFactory;
    }
    public MetaNumberParamPropertyViewModel Create(string propertyIdentifier, MetaNumberParam<double> target, double min = double.MinValue, double max = double.MaxValue, double recommendedMin = double.MinValue, double recommendedMax = double.MaxValue, bool allowMultiClipApply = true, IMetasiaObject? owner = null)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        return new MetaNumberParamPropertyViewModel(coordPointViewModelFactory, selectionState, propertyIdentifier, editCommandManager, projectState, target, min, max, recommendedMin, recommendedMax, allowMultiClipApply, owner);
    }
}
