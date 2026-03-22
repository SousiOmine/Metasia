using Avalonia.Controls;
using Avalonia.Media;

namespace Metasia.Editor.Plugin;

public sealed class LeftPanePanelDefinition
{
    public LeftPanePanelDefinition(
        string id,
        string title,
        Func<Control> createView,
        string? tooltip = null,
        Geometry? icon = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(createView);

        Id = id;
        Title = title;
        Tooltip = string.IsNullOrWhiteSpace(tooltip) ? title : tooltip;
        Icon = icon;
        CreateView = createView;
    }

    public string Id { get; }

    public string Title { get; }

    public string Tooltip { get; }

    public Geometry? Icon { get; }

    public Func<Control> CreateView { get; }
}
