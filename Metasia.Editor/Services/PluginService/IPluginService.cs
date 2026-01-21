using System.Collections.Generic;
using System.Threading.Tasks;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Services.PluginService
{
    public interface IPluginService
    {
        List<IEditorPlugin> EditorPlugins { get; }

        List<IMediaInputPlugin> MediaInputPlugins { get; }

        List<IMediaOutputPlugin> MediaOutputPlugins { get; }

        Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync();
    }
}