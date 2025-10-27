using System.Collections.Generic;

namespace Metasia.Editor.Services;

public interface IFontCatalogService
{
    IReadOnlyList<string> GetInstalledFonts();
}
