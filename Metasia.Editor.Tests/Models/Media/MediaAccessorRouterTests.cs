using Metasia.Core.Media;
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
        public void Constructor_RegistersStdInput()
        {
            var settingsService = new FakeSettingsService();
            var router = new MediaAccessorRouter(settingsService);
            var infos = router.GetRegisteredAccessorInfos();

            Assert.That(infos.Any(x => x.Id == MediaAccessorRouter.StdInputAccessorId), Is.True);
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

        private sealed class DualAccessor : IMediaAccessor, IImageFileAccessor, IVideoFileAccessor
        {
            private readonly bool _imageSuccessful;
            private readonly bool _videoSuccessful;

            public int ImageCallCount { get; private set; }
            public int VideoByTimeCallCount { get; private set; }
            public int VideoByFrameCallCount { get; private set; }

            public DualAccessor(bool imageSuccessful, bool videoSuccessful)
            {
                _imageSuccessful = imageSuccessful;
                _videoSuccessful = videoSuccessful;
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
    }
}
