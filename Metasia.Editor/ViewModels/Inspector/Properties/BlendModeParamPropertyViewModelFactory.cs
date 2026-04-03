using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Render;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class BlendModeParamPropertyViewModelFactory : IBlendModeParamPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public BlendModeParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState)
    {
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
    }

    public BlendModeParamPropertyViewModel Create(string propertyIdentifier, BlendModeParam target)
    {
        return new BlendModeParamPropertyViewModel(
            _selectionState,
            propertyIdentifier,
            _editCommandManager,
            _projectState,
            target);
    }
}