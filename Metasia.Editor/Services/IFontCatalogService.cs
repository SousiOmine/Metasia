using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Collections.Generic;

namespace Metasia.Editor.Services;

public interface IFontCatalogService
{
    IReadOnlyList<string> GetInstalledFonts();
}
