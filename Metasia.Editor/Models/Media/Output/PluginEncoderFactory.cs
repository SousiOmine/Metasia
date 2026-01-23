namespace Metasia.Editor.Models.Media.Output;

using Metasia.Editor.Models.Media;
using Metasia.Editor.Plugin;

public class PluginEncoderFactory : IEditorEncoderFactory
{
    private readonly IMediaOutputPlugin _plugin;

    public PluginEncoderFactory(IMediaOutputPlugin plugin)
    {
        _plugin = plugin;
    }

    public string Name => _plugin.Name;

    public string[] SupportedExtensions => _plugin.SupportedExtensions;

    public IEditorEncoder CreateEncoder()
    {
        return new PluginEncoder(_plugin);
    }
}
