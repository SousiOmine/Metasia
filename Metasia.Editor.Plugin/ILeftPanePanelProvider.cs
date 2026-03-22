namespace Metasia.Editor.Plugin;

public interface ILeftPanePanelProvider
{
    IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels();
}
