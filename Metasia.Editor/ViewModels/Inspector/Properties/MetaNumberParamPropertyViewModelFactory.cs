using System;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaNumberParamPropertyViewModelFactory : IMetaNumberParamPropertyViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState projectState;
    public MetaNumberParamPropertyViewModelFactory(ISelectionState selectionState, IEditCommandManager editCommandManager, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        this.selectionState = selectionState;
        this.editCommandManager = editCommandManager;
        this.projectState = projectState;
    }
    public MetaNumberParamPropertyViewModel Create(string propertyIdentifier, MetaNumberParam<double> target, double min = double.MinValue, double max = double.MaxValue, double recommendedMin = double.MinValue, double recommendedMax = double.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        return new MetaNumberParamPropertyViewModel(selectionState, propertyIdentifier, editCommandManager, projectState, target, min, max, recommendedMin, recommendedMax);
    }
}
