using Metasia.Core.Media;
using Metasia.Core.Sounds;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Services;

namespace Metasia.Editor.Tests.Models.Media
{
    [TestFixture]
    public class MediaAccessorRouterTests
    {
        [Test]
        public async Task ApplyPriorityOrder_AffectsImageAndVideoRouting()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: true, videoSuccessful: true);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);
            router.ApplyPriorityOrder(["plugin.b", "plugin.a"]);

            var image = await router.GetImageAsync("missing.file");
            var video = await router.GetImageAsync("missing.file", TimeSpan.Zero);

            Assert.That(image.IsSuccessful, Is.True);
            Assert.That(video.IsSuccessful, Is.True);
            Assert.That(accessorB.ImageCallCount, Is.EqualTo(1));
            Assert.That(accessorB.VideoByTimeCallCount, Is.EqualTo(1));
            Assert.That(accessorA.ImageCallCount, Is.EqualTo(0));
            Assert.That(accessorA.VideoByTimeCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task ApplyPriorityOrder_AppendsUnconfiguredAccessorsInRegistrationOrder()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: true, videoSuccessful: true);
            var accessorC = new DualAccessor(imageSuccessful: false, videoSuccessful: false);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);
            router.RegisterAccessor("plugin.c", "Plugin C", accessorC);
            router.ApplyPriorityOrder(["plugin.c"]);

            var image = await router.GetImageAsync("missing.file");

            Assert.That(image.IsSuccessful, Is.True);
            Assert.That(accessorC.ImageCallCount, Is.EqualTo(1));
            Assert.That(accessorA.ImageCallCount, Is.EqualTo(1));
            Assert.That(accessorB.ImageCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ApplyPriorityOrder_DeduplicatesAndIgnoresUnknownIds()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: true, videoSuccessful: true);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);
            router.ApplyPriorityOrder(["unknown.id", "plugin.b", "plugin.b"]);

            var image = await router.GetImageAsync("missing.file");

            Assert.That(image.IsSuccessful, Is.True);
            Assert.That(accessorB.ImageCallCount, Is.EqualTo(1));
            Assert.That(accessorA.ImageCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task Constructor_RegistersStdInput()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var infos = router.GetRegisteredAccessorInfos();

            Assert.That(infos.Any(x => x.Id == MediaAccessorRouter.StdInputAccessorId), Is.True);
        }

        [Test]
        public async Task GetAudioAsync_NoAudioAccessor_ReturnsUnsuccessful()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessor = new DualAccessor(imageSuccessful: true, videoSuccessful: true, audioSuccessful: false);

            router.RegisterAccessor("plugin.test", "Test Plugin", accessor);

            var result = await router.GetAudioAsync("test.mp3");

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Chunk, Is.Null);
            Assert.That(accessor.AudioCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAudioAsync_WithSuccessfulAccessor_ReturnsSuccessful()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false, audioSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: false, videoSuccessful: false, audioSuccessful: true);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);
            router.ApplyPriorityOrder(["plugin.b", "plugin.a"]);

            var result = await router.GetAudioAsync("test.mp3");

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Chunk, Is.Not.Null);
            Assert.That(accessorB.AudioCallCount, Is.EqualTo(1));
            Assert.That(accessorA.AudioCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task ApplyPriorityOrder_AffectsAudioRouting()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false, audioSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: false, videoSuccessful: false, audioSuccessful: true);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);
            router.ApplyPriorityOrder(["plugin.b", "plugin.a"]);

            var result = await router.GetAudioAsync("test.mp3");

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(accessorB.AudioCallCount, Is.EqualTo(1));
            Assert.That(accessorA.AudioCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAudioAsync_PassesStartTimeAndDuration()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessor = new AudioAccessorWithParams();

            router.RegisterAccessor("plugin.test", "Test Plugin", accessor);

            var result = await router.GetAudioAsync("test.mp3", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

            Assert.That(accessor.LastPath, Is.EqualTo("test.mp3"));
            Assert.That(accessor.LastStartTime, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(accessor.LastDuration, Is.EqualTo(TimeSpan.FromSeconds(10)));
        }

        [Test]
        public async Task SettingsChanged_ReordersAccessors()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var accessorA = new DualAccessor(imageSuccessful: false, videoSuccessful: false);
            var accessorB = new DualAccessor(imageSuccessful: true, videoSuccessful: true);

            router.RegisterAccessor("plugin.a", "Plugin A", accessorA);
            router.RegisterAccessor("plugin.b", "Plugin B", accessorB);

            settingsService.CurrentSettings.Editor.MediaAccessorPriorityOrder = ["plugin.b", "plugin.a"];
            settingsService.RaiseSettingsChanged();

            var image = await router.GetImageAsync("missing.file");

            Assert.That(image.IsSuccessful, Is.True);
            Assert.That(accessorB.ImageCallCount, Is.EqualTo(1));
            Assert.That(accessorA.ImageCallCount, Is.EqualTo(0));
        }

        private sealed class DualAccessor : IMediaAccessor, IImageFileAccessor, IVideoFileAccessor, IAudioFileAccessor
        {
            private readonly bool _imageSuccessful;
            private readonly bool _videoSuccessful;
            private readonly bool _audioSuccessful;

            public int ImageCallCount { get; private set; }
            public int VideoByTimeCallCount { get; private set; }
            public int VideoByFrameCallCount { get; private set; }
            public int AudioCallCount { get; private set; }

            public DualAccessor(bool imageSuccessful, bool videoSuccessful, bool audioSuccessful = false)
            {
                _imageSuccessful = imageSuccessful;
                _videoSuccessful = videoSuccessful;
                _audioSuccessful = audioSuccessful;
            }

            public Task<ImageFileAccessorResult> GetImageAsync(string path)
            {
                ImageCallCount++;
                return Task.FromResult(new ImageFileAccessorResult { IsSuccessful = _imageSuccessful });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
            {
                VideoByTimeCallCount++;
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = _videoSuccessful });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
            {
                VideoByFrameCallCount++;
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = _videoSuccessful });
            }

            public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            {
                AudioCallCount++;
                return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = _audioSuccessful, Chunk = _audioSuccessful ? new AudioChunk(new AudioFormat(44100, 2), 0) : null });
            }
        }

        private sealed class FakeSettingsService : ISettingsService
        {
            public EditorSettings CurrentSettings { get; private set; } = new();
            public event Action? SettingsChanged;

            public Task LoadAsync() => Task.CompletedTask;
            public Task SaveAsync() => Task.CompletedTask;

            public Task UpdateSettingsAsync(EditorSettings settings)
            {
                CurrentSettings = settings;
                SettingsChanged?.Invoke();
                return Task.CompletedTask;
            }

            public void RaiseSettingsChanged()
            {
                SettingsChanged?.Invoke();
            }
        }

        private sealed class AudioAccessorWithParams : IMediaAccessor, IAudioFileAccessor
        {
            public string? LastPath { get; private set; }
            public TimeSpan? LastStartTime { get; private set; }
            public TimeSpan? LastDuration { get; private set; }

            public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            {
                LastPath = path;
                LastStartTime = startTime;
                LastDuration = duration;
                return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = true, Chunk = new AudioChunk(new AudioFormat(44100, 2), 0) });
            }
        }
    }
}

