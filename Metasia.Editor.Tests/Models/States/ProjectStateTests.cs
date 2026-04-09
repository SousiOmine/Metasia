using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using NUnit.Framework;
using System.IO;

namespace Metasia.Editor.Tests.Models.States;

[TestFixture]
public class ProjectStateTests
{
    [Test]
    public void LoadProjectAsync_SelectsRootTimelineWhenPresent()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var state = new ProjectState();
            var rootTimeline = new TimelineObject("RootTimeline");
            var secondTimeline = new TimelineObject("Timeline2");
            var project = new MetasiaEditorProject(
                new DirectoryEntity(tempDirectory),
                new MetasiaProjectFile
                {
                    RootTimelineId = rootTimeline.Id
                });

            project.Timelines.Add(secondTimeline);
            project.Timelines.Add(rootTimeline);

            state.LoadProjectAsync(project).GetAwaiter().GetResult();

            Assert.That(state.CurrentTimeline, Is.SameAs(rootTimeline));
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void LoadProjectAsync_FallsBackToFirstTimelineWhenRootTimelineIsMissing()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var state = new ProjectState();
            var firstTimeline = new TimelineObject("Timeline1");
            var secondTimeline = new TimelineObject("Timeline2");
            var project = new MetasiaEditorProject(
                new DirectoryEntity(tempDirectory),
                new MetasiaProjectFile
                {
                    RootTimelineId = "MissingTimeline"
                });

            project.Timelines.Add(firstTimeline);
            project.Timelines.Add(secondTimeline);

            state.LoadProjectAsync(project).GetAwaiter().GetResult();

            Assert.That(state.CurrentTimeline, Is.SameAs(firstTimeline));
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        return path;
    }
}
