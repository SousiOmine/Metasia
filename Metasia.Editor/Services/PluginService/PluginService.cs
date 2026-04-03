using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Metasia.Core.Attributes;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Core.Xml;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Plugins;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Services.PluginService
{
    public class PluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; private set; } = [];

        public List<IMediaInputPlugin> MediaInputPlugins { get; private set; } = [];

        public List<IMediaOutputPlugin> MediaOutputPlugins { get; private set; } = [];

        public IReadOnlyList<PluginTypeInfo> PluginClipTypes => _pluginClipTypes;
        public IReadOnlyList<PluginTypeInfo> PluginVisualEffectTypes => _pluginVisualEffectTypes;
        public IReadOnlyList<PluginTypeInfo> PluginAudioEffectTypes => _pluginAudioEffectTypes;

        private readonly List<PluginTypeInfo> _pluginClipTypes = [];
        private readonly List<PluginTypeInfo> _pluginVisualEffectTypes = [];
        private readonly List<PluginTypeInfo> _pluginAudioEffectTypes = [];

        private readonly MediaAccessorRouter _mediaAccessorRouter;
        private readonly TypeRegistry _typeRegistry;
        private readonly EditorHostContext _hostContext;
        private readonly Func<Task<IEnumerable<IEditorPlugin>>> _pluginLoader;

        public PluginService(
            MediaAccessorRouter mediaAccessorRouter,
            TypeRegistry typeRegistry,
            IEditCommandManager editCommandManager,
            ISelectionState selectionState,
            ITimelineViewState timelineViewState,
            IPlaybackState playbackState,
            INotificationService notificationService,
            Func<Task<IEnumerable<IEditorPlugin>>>? pluginLoader = null)
        {
            _mediaAccessorRouter = mediaAccessorRouter;
            _typeRegistry = typeRegistry;
            _hostContext = new EditorHostContext(editCommandManager, selectionState, timelineViewState, playbackState, notificationService);
            _pluginLoader = pluginLoader ?? PluginLoader.LoadEditorPluginsAsync;
        }

        public async Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            Debug.WriteLine("Loading plugins...");
            try
            {
                EditorPlugins.Clear();
                MediaInputPlugins.Clear();
                MediaOutputPlugins.Clear();
                _pluginClipTypes.Clear();
                _pluginVisualEffectTypes.Clear();
                _pluginAudioEffectTypes.Clear();

                EditorPlugins = (await _pluginLoader()).ToList();
                foreach (var plugin in EditorPlugins)
                {
                    plugin.Initialize(_hostContext);
                    if (plugin is IMediaInputPlugin mediaInputPlugin)
                    {
                        MediaInputPlugins.Add(mediaInputPlugin);
                    }
                    if (plugin is IMediaOutputPlugin mediaOutputPlugin)
                    {
                        MediaOutputPlugins.Add(mediaOutputPlugin);
                    }
                }
                RegisterMediaInputPlugins();
                RegisterMediaOutputPlugins();
                RegisterPluginTypes();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Debug.WriteLine($"{EditorPlugins.Count} plugins loaded.");
            return EditorPlugins;
        }

        private void RegisterPluginTypes()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is IClipTypeProvider clipProvider)
                {
                    RegisterClipTypes(plugin, clipProvider);
                }
                if (plugin is IVisualEffectTypeProvider visualEffectProvider)
                {
                    RegisterVisualEffectTypes(plugin, visualEffectProvider);
                }
                if (plugin is IAudioEffectTypeProvider audioEffectProvider)
                {
                    RegisterAudioEffectTypes(plugin, audioEffectProvider);
                }
            }
        }

        private void RegisterClipTypes(IEditorPlugin plugin, IClipTypeProvider provider)
        {
            IEnumerable<Type> types;
            try
            {
                types = provider.GetClipTypes() ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get clip types from {plugin.PluginIdentifier}: {ex}");
                return;
            }

            foreach (var type in types)
            {
                if (!TryValidatePluginType(type, typeof(ClipObject), typeof(ClipTypeIdentifierAttribute), plugin, out var attribute))
                    continue;

                var identifier = ((ClipTypeIdentifierAttribute)attribute).Identifier;
                var typeId = $"{plugin.PluginIdentifier}:{identifier}";
                _typeRegistry.Register(plugin.PluginIdentifier, identifier, type);

                var displayName = DisplayTextResolver.ResolveClipDisplayName(type);
                _pluginClipTypes.Add(new PluginTypeInfo
                {
                    TypeId = typeId,
                    DisplayName = displayName,
                    PluginName = plugin.PluginName,
                    PluginIdentifier = plugin.PluginIdentifier,
                    Type = type
                });
            }
        }

        private void RegisterVisualEffectTypes(IEditorPlugin plugin, IVisualEffectTypeProvider provider)
        {
            IEnumerable<Type> types;
            try
            {
                types = provider.GetVisualEffectTypes() ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get visual effect types from {plugin.PluginIdentifier}: {ex}");
                return;
            }

            foreach (var type in types)
            {
                if (!TryValidatePluginType(type, typeof(VisualEffectBase), typeof(VisualEffectIdentifierAttribute), plugin, out var attribute))
                    continue;

                var identifier = ((VisualEffectIdentifierAttribute)attribute).Identifier;
                var typeId = $"{plugin.PluginIdentifier}:{identifier}";
                _typeRegistry.Register(plugin.PluginIdentifier, identifier, type);

                var displayName = DisplayTextResolver.ResolveVisualEffectDisplayName(type);
                _pluginVisualEffectTypes.Add(new PluginTypeInfo
                {
                    TypeId = typeId,
                    DisplayName = displayName,
                    PluginName = plugin.PluginName,
                    PluginIdentifier = plugin.PluginIdentifier,
                    Type = type
                });
            }
        }

        private void RegisterAudioEffectTypes(IEditorPlugin plugin, IAudioEffectTypeProvider provider)
        {
            IEnumerable<Type> types;
            try
            {
                types = provider.GetAudioEffectTypes() ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get audio effect types from {plugin.PluginIdentifier}: {ex}");
                return;
            }

            foreach (var type in types)
            {
                if (!TryValidatePluginType(type, typeof(AudioEffectBase), typeof(AudioEffectIdentifierAttribute), plugin, out var attribute))
                    continue;

                var identifier = ((AudioEffectIdentifierAttribute)attribute).Identifier;
                var typeId = $"{plugin.PluginIdentifier}:{identifier}";
                _typeRegistry.Register(plugin.PluginIdentifier, identifier, type);

                var displayName = DisplayTextResolver.ResolveAudioEffectDisplayName(type);
                _pluginAudioEffectTypes.Add(new PluginTypeInfo
                {
                    TypeId = typeId,
                    DisplayName = displayName,
                    PluginName = plugin.PluginName,
                    PluginIdentifier = plugin.PluginIdentifier,
                    Type = type
                });
            }
        }

        private bool TryValidatePluginType(Type type, Type requiredBaseType, Type requiredAttributeType, IEditorPlugin plugin, out Attribute? attribute)
        {
            attribute = null;

            if (!requiredBaseType.IsAssignableFrom(type))
            {
                Debug.WriteLine($"Plugin {plugin.PluginIdentifier}: Type {type.Name} does not inherit from {requiredBaseType.Name}. Skipping.");
                return false;
            }

            if (type.IsAbstract || !type.IsClass)
            {
                Debug.WriteLine($"Plugin {plugin.PluginIdentifier}: Type {type.Name} is not a concrete class. Skipping.");
                return false;
            }

            if (type.GetConstructor(Type.EmptyTypes) is null)
            {
                Debug.WriteLine($"Plugin {plugin.PluginIdentifier}: Type {type.Name} does not have a public parameterless constructor. Skipping.");
                return false;
            }

            var existingType = _typeRegistry.GetTypeByTypeName(type.Name);
            if (existingType is not null && existingType != type)
            {
                Debug.WriteLine($"Plugin {plugin.PluginIdentifier}: Type name {type.Name} conflicts with {existingType.FullName}. Skipping.");
                return false;
            }

            var attr = type.GetCustomAttribute(requiredAttributeType);
            if (attr is null)
            {
                Debug.WriteLine($"Plugin {plugin.PluginIdentifier}: Type {type.Name} is missing {requiredAttributeType.Name}. Skipping.");
                return false;
            }

            attribute = attr;
            return true;
        }

        private void RegisterMediaInputPlugins()
        {
            foreach (var plugin in MediaInputPlugins)
            {
                _mediaAccessorRouter.RegisterAccessor(plugin.PluginIdentifier, plugin.PluginName, plugin);
            }
        }

        private void RegisterMediaOutputPlugins()
        {

        }

        public IEnumerable<IPluginSettingsProvider> GetSettingsProviders()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is IPluginSettingsProvider settingsProvider)
                {
                    yield return settingsProvider;
                }
            }
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is not ILeftPanePanelProvider panelProvider)
                {
                    continue;
                }

                var safePanels = new List<LeftPanePanelDefinition>();
                try
                {
                    foreach (var panel in panelProvider.GetLeftPanePanels() ?? [])
                    {
                        if (panel is null)
                        {
                            continue;
                        }

                        safePanels.Add(new LeftPanePanelDefinition(
                            $"{plugin.PluginIdentifier}:{panel.Id}",
                            panel.Title,
                            WrapCreateView(plugin, panel),
                            panel.Tooltip,
                            panel.Icon));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to get left pane panels from {plugin.PluginIdentifier}: {ex}");
                }

                foreach (var panel in safePanels)
                {
                    yield return panel;
                }
            }
        }

        private static Func<Control> WrapCreateView(IEditorPlugin plugin, LeftPanePanelDefinition panel)
        {
            return () =>
            {
                try
                {
                    return panel.CreateView();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create left pane view {panel.Id} from {plugin.PluginIdentifier}: {ex}");
                    throw;
                }
            };
        }
    }
}
