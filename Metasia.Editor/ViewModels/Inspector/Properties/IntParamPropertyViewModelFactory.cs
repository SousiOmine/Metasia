using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class IntParamPropertyViewModelFactory : IIntParamPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public IntParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public IntParamPropertyViewModel Create(string propertyIdentifier, MetaIntParam target, int min = int.MinValue, int max = int.MaxValue, int recommendMin = int.MinValue, int recommendMax = int.MaxValue, bool allowMultiClipApply = true, IMetasiaObject? owner = null)
    {
        return new IntParamPropertyViewModel(
            _selectionState,
            propertyIdentifier,
            _editCommandManager,
            _projectState,
            target,
            min,
            max,
            recommendMin,
            recommendMax,
            allowMultiClipApply,
            owner);
    }
}
