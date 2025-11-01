using System;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaFontParamPropertyViewModelFactory : IMetaFontParamPropertyViewModelFactory
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;
    private readonly IFontCatalogService _fontCatalogService;

    public MetaFontParamPropertyViewModelFactory(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        IFontCatalogService fontCatalogService)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(fontCatalogService);

        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _fontCatalogService = fontCatalogService;
    }

    public MetaFontParamPropertyViewModel Create(string propertyIdentifier, MetaFontParam target)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        var fonts = _fontCatalogService.GetInstalledFonts();
        return new MetaFontParamPropertyViewModel(
            _selectionState,
            _editCommandManager,
            _projectState,
            propertyIdentifier,
            target,
            fonts);
    }
}
