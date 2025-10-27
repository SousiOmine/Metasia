using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Core.Typography;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public class MetaFontParamPropertyViewModel : ViewModelBase
{
    public string PropertyDisplayName
    {
        get => _propertyDisplayName;
        set => this.RaiseAndSetIfChanged(ref _propertyDisplayName, value);
    }

    public string PropertyIdentifier
    {
        get => _propertyIdentifier;
        set => this.RaiseAndSetIfChanged(ref _propertyIdentifier, value);
    }

    public ObservableCollection<string> AvailableFonts { get; } = new();

    public string SelectedFont
    {
        get => _selectedFont;
        set
        {
            var sanitized = string.IsNullOrWhiteSpace(value) ? MetaFontParam.Default.FamilyName : value;
            if (_selectedFont == sanitized)
            {
                return;
            }
            EnsureFontExists(sanitized);
            this.RaiseAndSetIfChanged(ref _selectedFont, sanitized);
            ApplyFontChange();
        }
    }

    public bool IsBold
    {
        get => _isBold;
        set
        {
            if (_isBold == value)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _isBold, value);
            ApplyFontChange();
        }
    }

    public bool IsItalic
    {
        get => _isItalic;
        set
        {
            if (_isItalic == value)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _isItalic, value);
            ApplyFontChange();
        }
    }

    private string _propertyDisplayName = string.Empty;
    private string _propertyIdentifier = string.Empty;
    private string _selectedFont = MetaFontParam.Default.FamilyName;
    private bool _isBold;
    private bool _isItalic;
    private MetaFontParam _propertyValue;
    private bool _suppressChangeEvents;

    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly IProjectState _projectState;

    public MetaFontParamPropertyViewModel(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        string propertyIdentifier,
        MetaFontParam target,
        IEnumerable<string> installedFonts)
    {
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(installedFonts);

        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _projectState = projectState;
        _propertyIdentifier = propertyIdentifier;
        _propertyDisplayName = propertyIdentifier;
        _propertyValue = target.Clone();

        foreach (var font in installedFonts.Distinct(StringComparer.CurrentCultureIgnoreCase)
                                          .OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase))
        {
            EnsureFontExists(font);
        }

        EnsureFontExists(_propertyValue.FamilyName);

        _selectionState.SelectionChanged += OnSelectionChanged;
        _projectState.TimelineChanged += OnTimelineChanged;

        UpdateStateFromParam(_propertyValue);
    }

    private void ApplyFontChange()
    {
        var nextValue = BuildCurrentFontParam();
        if (_suppressChangeEvents)
        {
            _propertyValue = nextValue;
            return;
        }

        if (_propertyValue.Equals(nextValue))
        {
            return;
        }

        var before = _propertyValue.Clone();
        var after = nextValue.Clone();
        var command = TimelineInteractor.CreateFontParamValueChangeCommand(_propertyIdentifier, before, after, _selectionState.SelectedClips);
        if (command is not null)
        {
            _editCommandManager.Execute(command);
        }
        _propertyValue = after;
    }

    private MetaFontParam BuildCurrentFontParam()
    {
        return new MetaFontParam(
            string.IsNullOrWhiteSpace(_selectedFont) ? MetaFontParam.Default.FamilyName : _selectedFont,
            _isBold,
            _isItalic);
    }

    private void EnsureFontExists(string font)
    {
        if (string.IsNullOrWhiteSpace(font))
        {
            return;
        }
        if (AvailableFonts.Any(x => string.Equals(x, font, StringComparison.CurrentCultureIgnoreCase)))
        {
            return;
        }

        var insertIndex = 0;
        while (insertIndex < AvailableFonts.Count &&
               string.Compare(AvailableFonts[insertIndex], font, StringComparison.CurrentCultureIgnoreCase) < 0)
        {
            insertIndex++;
        }
        AvailableFonts.Insert(insertIndex, font);
    }

    private void OnSelectionChanged()
    {
        RefreshFromSelection();
    }

    private void OnTimelineChanged()
    {
        RefreshFromSelection();
    }

    private void RefreshFromSelection()
    {
        var clip = _selectionState.CurrentSelectedClip ?? _selectionState.SelectedClips.FirstOrDefault();
        if (clip is null)
        {
            return;
        }

        var properties = ObjectPropertyFinder.FindEditableProperties(clip);
        var property = properties.FirstOrDefault(x => x.Identifier == _propertyIdentifier && x.Type == typeof(MetaFontParam));
        if (property?.PropertyValue is MetaFontParam fontParam)
        {
            UpdateStateFromParam(fontParam);
        }
    }

    private void UpdateStateFromParam(MetaFontParam param)
    {
        _suppressChangeEvents = true;
        _propertyValue = param.Clone();
        EnsureFontExists(_propertyValue.FamilyName);
        SelectedFont = _propertyValue.FamilyName;
        IsBold = _propertyValue.IsBold;
        IsItalic = _propertyValue.IsItalic;
        _suppressChangeEvents = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _selectionState.SelectionChanged -= OnSelectionChanged;
            _projectState.TimelineChanged -= OnTimelineChanged;
        }

        base.Dispose(disposing);
    }
}
