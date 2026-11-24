using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using Moq;
using NUnit.Framework;
using SkiaSharp;

namespace Metasia.Editor.Tests.Models.States;

[TestFixture]
public class PlaybackStateTests
{
    private static PlaybackState CreatePlaybackState(ProjectInfo? projectInfo)
    {
        var projectState = new Mock<IProjectState>();
        projectState.Setup(x => x.CurrentProjectInfo).Returns(projectInfo);

        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(x => x.CurrentSettings).Returns(new EditorSettings());

        return new PlaybackState(projectState.Object, new FakeAudioPlaybackService(), new MediaAccessorRouter(settingsService.Object));
    }

    [Test]
    public void SamplingRate_FollowsProjectSetting()
    {
        var state = CreatePlaybackState(new ProjectInfo(60, new SKSize(1920, 1080), 48000, 2));

        Assert.That(state.SamplingRate, Is.EqualTo(48000));
    }

    [Test]
    public void AudioChannels_FollowsProjectSetting()
    {
        var state = CreatePlaybackState(new ProjectInfo(60, new SKSize(1920, 1080), 48000, 6));

        Assert.That(state.AudioChannels, Is.EqualTo(6));
    }

    [Test]
    public void AudioChannels_ReturnsDefaultWhenProjectInfoIsMissing()
    {
        var state = CreatePlaybackState(null);

        Assert.That(state.SamplingRate, Is.EqualTo(PlaybackState.DefaultSamplingRate));
        Assert.That(state.AudioChannels, Is.EqualTo(PlaybackState.DefaultAudioChannels));
    }

    [Test]
    public void AudioChannels_ReturnsDefaultWhenSamplingRateIsNotPositive()
    {
        var state = CreatePlaybackState(new ProjectInfo(60, new SKSize(1920, 1080), 0, 2));

        Assert.That(state.SamplingRate, Is.EqualTo(PlaybackState.DefaultSamplingRate));
    }

    private sealed class FakeAudioPlaybackService : IAudioPlaybackService
    {
        public bool IsPlaying { get; private set; }

        public long CurrentSample { get; private set; }

        public void Play(TimelineObject timeline, ProjectInfo projectInfo, long startSample, double speed, int samplingRate, int audioChannels, Metasia.Core.Media.IAudioFileAccessor audioFileAccessor, string projectPath, IReadOnlyDictionary<string, TimelineObject> availableTimelines)
        {
            IsPlaying = true;
            CurrentSample = startSample;
        }

        public void Pause()
        {
            IsPlaying = false;
        }
    }
}
