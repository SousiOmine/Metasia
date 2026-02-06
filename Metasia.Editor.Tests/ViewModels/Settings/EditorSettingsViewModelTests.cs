using Metasia.Core.Media;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels.Settings;

namespace Metasia.Editor.Tests.ViewModels.Settings
{
    [TestFixture]
    public class EditorSettingsViewModelTests
    {
        [Test]
        public void MovePriorityCommands_UpdateMediaAccessorPriorityOrder()
        {
            var router = CreateRouterWithTwoPlugins();
            var settings = new EditorSettings();
            settings.Editor.MediaAccessorPriorityOrder =
            [
                "plugin.b",
                "plugin.a",
                MediaAccessorRouter.StdInputAccessorId
            ];
            var vm = new EditorSettingsViewModel(settings, router);

            vm.SelectedMediaAccessorPriority = vm.MediaAccessorPriority[1];
            vm.MovePriorityUpCommand.Execute().Subscribe();

            Assert.That(settings.Editor.MediaAccessorPriorityOrder[0], Is.EqualTo("plugin.a"));
            Assert.That(settings.Editor.MediaAccessorPriorityOrder[1], Is.EqualTo("plugin.b"));
        }

        [Test]
        public void MovePriorityCommands_RespectBoundaryCanExecute()
        {
            var router = CreateRouterWithTwoPlugins();
            var settings = new EditorSettings();
            settings.Editor.MediaAccessorPriorityOrder =
            [
                "plugin.a",
                "plugin.b",
                MediaAccessorRouter.StdInputAccessorId
            ];
            var vm = new EditorSettingsViewModel(settings, router);

            vm.SelectedMediaAccessorPriority = vm.MediaAccessorPriority[0];
            Assert.That(((System.Windows.Input.ICommand)vm.MovePriorityUpCommand).CanExecute(null), Is.False);

            vm.SelectedMediaAccessorPriority = vm.MediaAccessorPriority[^1];
            Assert.That(((System.Windows.Input.ICommand)vm.MovePriorityDownCommand).CanExecute(null), Is.False);
        }

        [Test]
        public void MovePriorityCommands_RaiseSettingsEditedEvent()
        {
            var router = CreateRouterWithTwoPlugins();
            var settings = new EditorSettings();
            settings.Editor.MediaAccessorPriorityOrder =
            [
                "plugin.a",
                "plugin.b",
                MediaAccessorRouter.StdInputAccessorId
            ];
            var vm = new EditorSettingsViewModel(settings, router);
            var eventRaised = false;
            vm.SettingsEdited += () => eventRaised = true;

            vm.SelectedMediaAccessorPriority = vm.MediaAccessorPriority[0];
            vm.MovePriorityDownCommand.Execute().Subscribe();

            Assert.That(eventRaised, Is.True);
        }

        private static MediaAccessorRouter CreateRouterWithTwoPlugins()
        {
            var router = new MediaAccessorRouter(new FakeSettingsService());
            router.RegisterAccessor("plugin.a", "Plugin A", new DualAccessor());
            router.RegisterAccessor("plugin.b", "Plugin B", new DualAccessor());
            return router;
        }

        private sealed class DualAccessor : IMediaAccessor, IImageFileAccessor, IVideoFileAccessor
        {
            public Task<ImageFileAccessorResult> GetImageAsync(string path)
            {
                return Task.FromResult(new ImageFileAccessorResult { IsSuccessful = false });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
            {
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false });
            }

            public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
            {
                return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false });
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
        }
    }
}
