using System;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class BoolPropertyViewModelFactory : IBoolPropertyViewModelFactory
{
    private readonly ISelectionState selectionState;
    private readonly IEditCommandManager editCommandManager;

    public BoolPropertyViewModelFactory(ISelectionState selectionState, IEditCommandManager editCommandManager)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        this.selectionState = selectionState;
        this.editCommandManager = editCommandManager;
    }

    public BoolPropertyViewModel Create(string propertyIdentifier, bool target)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        return new BoolPropertyViewModel(selectionState, propertyIdentifier, editCommandManager, target);
    }
}