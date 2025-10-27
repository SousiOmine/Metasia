using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Metasia.Core.Typography;

namespace Metasia.Editor.Services;

public class FontCatalogService : IFontCatalogService
{
    private IReadOnlyList<string>? _cachedFonts;
    private readonly object _lock = new();

    public IReadOnlyList<string> GetInstalledFonts()
    {
        if (_cachedFonts is not null)
        {
            return _cachedFonts;
        }

        lock (_lock)
        {
            if (_cachedFonts is not null)
            {
                return _cachedFonts;
            }

            var fonts = FontManager.Current.SystemFonts
                .Select(font => font.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var defaultFont = MetaFontParam.Default.FamilyName;
            if (!fonts.Any(name => string.Equals(name, defaultFont, StringComparison.CurrentCultureIgnoreCase)))
            {
                fonts.Insert(0, defaultFont);
            }

            _cachedFonts = fonts;
            return _cachedFonts;
        }
    }
}
