using System.Reflection;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services.PluginService;

namespace Metasia.Editor.Tests.Services.PluginService
{
    [TestFixture]
    public class PluginServiceTests
    {
        [Test]
        public void RegisterMediaInputPlugins_DoesNotDuplicateStdInput()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var service = new Metasia.Editor.Services.PluginService.PluginService(router);

            service.MediaInputPlugins.Add(new FakeMediaInputPlugin("plugin.a", "Plugin A"));

            InvokePrivateRegisterMediaInputPlugins(service);

            var infos = router.GetRegisteredAccessorInfos();
            Assert.That(infos.Count(x => x.Id == MediaAccessorRouter.StdInputAccessorId), Is.EqualTo(1));
            Assert.That(infos.Count(x => x.Id == "plugin.a"), Is.EqualTo(1));
        }

        private static void InvokePrivateRegisterMediaInputPlugins(Metasia.Editor.Services.PluginService.PluginService service)
        {
            var method = typeof(Metasia.Editor.Services.PluginService.PluginService)
                .GetMethod("RegisterMediaInputPlugins", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            method!.Invoke(service, null);
        }

        private sealed class FakeSettingsService : Metasia.Editor.Services.ISettingsService
        {
            public Metasia.Editor.Models.Settings.EditorSettings CurrentSettings { get; private set; } = new();
            public event Action? SettingsChanged;

            public Task LoadAsync() => Task.CompletedTask;
            public Task SaveAsync() => Task.CompletedTask;

            public Task UpdateSettingsAsync(Metasia.Editor.Models.Settings.EditorSettings settings)
            {
                CurrentSettings = settings;
                SettingsChanged?.Invoke();
                return Task.CompletedTask;
            }
        }

        private sealed class FakeMediaInputPlugin : IMediaInputPlugin
        {
            public string PluginIdentifier { get; }
            public string PluginVersion { get; } = "1.0.0";
            public string PluginName { get; }

            public IEnumerable<IEditorPlugin.SupportEnvironment> SupportedEnvironments { get; } =
            [
                IEditorPlugin.SupportEnvironment.Windows_x64
            ];

            public int ImageCallCount { get; private set; }

            public FakeMediaInputPlugin(string id, string name)
            {
                PluginIdentifier = id;
                PluginName = name;
            }

            public void Initialize()
            {
            }

            public Task<ImageFileAccessorResult> GetImageAsync(string path)
            {
                ImageCallCount++;
                return Task.FromResult(new ImageFileAccessorResult { IsSuccessful = true });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
            {
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = true });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
            {
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = true });
            }

            public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            {
                return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });
            }
            
            public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
            {
                return Task.FromResult(new AudioSampleResult { IsSuccessful = false, Chunk = null });
            }
        }
    }
}
