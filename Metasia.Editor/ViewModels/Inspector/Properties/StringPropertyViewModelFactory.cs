using System;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class StringPropertyViewModelFactory : IStringPropertyViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState projectState;
    
    public StringPropertyViewModelFactory(ISelectionState selectionState, IEditCommandManager editCommandManager, IProjectState projectState)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        this.selectionState = selectionState;
        this.editCommandManager = editCommandManager;
        this.projectState = projectState;
    }
    
    public StringPropertyViewModel Create(string propertyIdentifier, string target)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        return new StringPropertyViewModel(selectionState, propertyIdentifier, editCommandManager, projectState, target);
    }
}