using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;

namespace Metasia.Editor.Tests.Models.States;

[TestFixture]
public class PersistentTimelineViewStateStoreTests
{
    [Test]
    public void ViewStateChanges_ArePersistedPerProjectFile()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var projectPath = Path.Combine(tempDirectory, "sample.mtpj");
            var projectState = new ProjectState();
            var repository = new ProjectTimelineViewStateRepository(tempDirectory);
            var store = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                projectState,
                TimeSpan.Zero);
            var rootTimeline = new TimelineObject("RootTimeline");
            var secondTimeline = new TimelineObject("Timeline2");

            projectState.LoadProjectAsync(CreateProject(projectPath, rootTimeline, secondTimeline)).GetAwaiter().GetResult();

            var rootState = store.GetViewState(rootTimeline.Id);
            rootState.Frame_Per_DIP = 5.5;
            rootState.LastPreviewFrame = 42;
            rootState.HorizontalScrollPosition = 12;

            var secondState = store.GetViewState(secondTimeline.Id);
            secondState.Frame_Per_DIP = 2.5;
            secondState.LastPreviewFrame = 24;
            secondState.HorizontalScrollPosition = 7;

            var savedJson = File.ReadAllText(repository.GetStateFilePath(projectPath));
            var snapshot = JsonSerializer.Deserialize<ProjectTimelineViewStateSnapshot>(savedJson);

            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot!.ProjectFilePath, Is.EqualTo(projectPath));

            var savedRoot = snapshot.Timelines.Single(x => x.TimelineId == rootTimeline.Id);
            var savedSecond = snapshot.Timelines.Single(x => x.TimelineId == secondTimeline.Id);

            Assert.Multiple(() =>
            {
                Assert.That(savedRoot.FramePerDip, Is.EqualTo(5.5));
                Assert.That(savedRoot.LastPreviewFrame, Is.EqualTo(42));
                Assert.That(savedRoot.HorizontalScrollPosition, Is.EqualTo(12));
                Assert.That(savedSecond.FramePerDip, Is.EqualTo(2.5));
                Assert.That(savedSecond.LastPreviewFrame, Is.EqualTo(24));
                Assert.That(savedSecond.HorizontalScrollPosition, Is.EqualTo(7));
            });
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void LoadProjectAsync_RestoresSavedStateForAllTimelines()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var projectPath = Path.Combine(tempDirectory, "sample.mtpj");
            var initialProjectState = new ProjectState();
            var repository = new ProjectTimelineViewStateRepository(tempDirectory);
            var initialStore = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                initialProjectState,
                TimeSpan.Zero);
            var initialRoot = new TimelineObject("RootTimeline");
            var initialSecond = new TimelineObject("Timeline2");

            initialProjectState.LoadProjectAsync(CreateProject(projectPath, initialRoot, initialSecond)).GetAwaiter().GetResult();
            initialStore.GetViewState(initialRoot.Id).Frame_Per_DIP = 6.0;
            initialStore.GetViewState(initialRoot.Id).LastPreviewFrame = 42;
            initialStore.GetViewState(initialRoot.Id).HorizontalScrollPosition = 18;
            initialStore.GetViewState(initialSecond.Id).Frame_Per_DIP = 2.0;
            initialStore.GetViewState(initialSecond.Id).LastPreviewFrame = 24;
            initialStore.GetViewState(initialSecond.Id).HorizontalScrollPosition = 9;

            var reloadedProjectState = new ProjectState();
            var reloadedStore = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                reloadedProjectState,
                TimeSpan.Zero);
            var reloadedRoot = new TimelineObject("RootTimeline");
            var reloadedSecond = new TimelineObject("Timeline2");

            reloadedProjectState.LoadProjectAsync(CreateProject(projectPath, reloadedRoot, reloadedSecond)).GetAwaiter().GetResult();

            var rootState = reloadedStore.GetViewState(reloadedRoot.Id);
            var secondState = reloadedStore.GetViewState(reloadedSecond.Id);

            Assert.Multiple(() =>
            {
                Assert.That(reloadedProjectState.CurrentTimeline, Is.SameAs(reloadedRoot));
                Assert.That(rootState.Frame_Per_DIP, Is.EqualTo(6.0));
                Assert.That(rootState.LastPreviewFrame, Is.EqualTo(42));
                Assert.That(rootState.HorizontalScrollPosition, Is.EqualTo(18));
                Assert.That(secondState.Frame_Per_DIP, Is.EqualTo(2.0));
                Assert.That(secondState.LastPreviewFrame, Is.EqualTo(24));
                Assert.That(secondState.HorizontalScrollPosition, Is.EqualTo(9));
            });
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void LoadProjectAsync_WhenSnapshotFileIsBroken_UsesDefaultState()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var projectPath = Path.Combine(tempDirectory, "sample.mtpj");
            var repository = new ProjectTimelineViewStateRepository(tempDirectory);
            var snapshotPath = repository.GetStateFilePath(projectPath);
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            File.WriteAllText(snapshotPath, "{ broken json");

            var projectState = new ProjectState();
            var store = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                projectState,
                TimeSpan.Zero);
            var rootTimeline = new TimelineObject("RootTimeline");

            Assert.DoesNotThrow(() =>
                projectState.LoadProjectAsync(CreateProject(projectPath, rootTimeline)).GetAwaiter().GetResult());

            var state = store.GetViewState(rootTimeline.Id);

            Assert.Multiple(() =>
            {
                Assert.That(state.Frame_Per_DIP, Is.EqualTo(3.0));
                Assert.That(state.LastPreviewFrame, Is.EqualTo(0));
                Assert.That(state.HorizontalScrollPosition, Is.EqualTo(0));
            });
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void ViewStateChanges_AreDebouncedBeforePersisting()
    {
        var projectState = new ProjectState();
        var repository = new RecordingRepository();
        using var store = new PersistentTimelineViewStateStore(
            new TimelineViewStateStore(),
            repository,
            projectState,
            TimeSpan.FromMilliseconds(80));
        var rootTimeline = new TimelineObject("RootTimeline");
        var projectPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "sample.mtpj");

        projectState.LoadProjectAsync(CreateProject(projectPath, rootTimeline)).GetAwaiter().GetResult();

        var state = store.GetViewState(rootTimeline.Id);
        state.LastPreviewFrame = 10;
        state.LastPreviewFrame = 20;
        state.LastPreviewFrame = 30;

        Assert.That(repository.SaveCallCount, Is.EqualTo(0));
        Assert.That(SpinWait.SpinUntil(() => repository.SaveCallCount == 1, TimeSpan.FromSeconds(2)), Is.True);
        Assert.That(repository.LastSavedSnapshot?.Timelines.Single().LastPreviewFrame, Is.EqualTo(30));
    }

    [Test]
    public void CloseProject_PersistsDirtyStateAfterProjectFilePathIsAssigned()
    {
        var projectState = new ProjectState();
        var repository = new RecordingRepository();
        using var store = new PersistentTimelineViewStateStore(
            new TimelineViewStateStore(),
            repository,
            projectState,
            TimeSpan.FromMilliseconds(30));
        var rootTimeline = new TimelineObject("RootTimeline");
        var project = CreateProjectWithoutFilePath(rootTimeline);
        var savePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "saved.mtpj");

        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        var state = store.GetViewState(rootTimeline.Id);
        state.Frame_Per_DIP = 4.5;
        state.LastPreviewFrame = 42;
        state.HorizontalScrollPosition = 9;

        Thread.Sleep(150);
        Assert.That(repository.SaveCallCount, Is.EqualTo(0));

        project.ProjectFilePath = savePath;
        projectState.CloseProject();

        Assert.That(repository.SaveCallCount, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(repository.LastSavedSnapshot?.ProjectFilePath, Is.EqualTo(savePath));
            Assert.That(repository.LastSavedSnapshot?.Timelines.Single().FramePerDip, Is.EqualTo(4.5));
            Assert.That(repository.LastSavedSnapshot?.Timelines.Single().LastPreviewFrame, Is.EqualTo(42));
            Assert.That(repository.LastSavedSnapshot?.Timelines.Single().HorizontalScrollPosition, Is.EqualTo(9));
        });
    }

    private static MetasiaEditorProject CreateProject(string projectFilePath, params TimelineObject[] timelines)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath)!;
        Directory.CreateDirectory(projectDirectory);

        var project = new MetasiaEditorProject(
            new DirectoryEntity(projectDirectory),
            new MetasiaProjectFile
            {
                RootTimelineId = "RootTimeline"
            })
        {
            ProjectFilePath = projectFilePath
        };

        foreach (var timeline in timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
    }

    private static MetasiaEditorProject CreateProjectWithoutFilePath(params TimelineObject[] timelines)
    {
        var projectDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(projectDirectory);

        var project = new MetasiaEditorProject(
            new DirectoryEntity(projectDirectory),
            new MetasiaProjectFile
            {
                RootTimelineId = "RootTimeline"
            });

        foreach (var timeline in timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class RecordingRepository : IProjectTimelineViewStateRepository
    {
        public int SaveCallCount { get; private set; }

        public ProjectTimelineViewStateSnapshot? LastSavedSnapshot { get; private set; }

        public ProjectTimelineViewStateSnapshot? Load(string projectFilePath)
        {
            return null;
        }

        public void Save(ProjectTimelineViewStateSnapshot snapshot)
        {
            LastSavedSnapshot = new ProjectTimelineViewStateSnapshot
            {
                ProjectFilePath = snapshot.ProjectFilePath,
                Timelines = snapshot.Timelines
                    .Select(timeline => new TimelineViewStateSnapshot
                    {
                        TimelineId = timeline.TimelineId,
                        FramePerDip = timeline.FramePerDip,
                        LastPreviewFrame = timeline.LastPreviewFrame,
                        HorizontalScrollPosition = timeline.HorizontalScrollPosition
                    })
                    .ToList()
            };
            SaveCallCount++;
        }
    }
}
