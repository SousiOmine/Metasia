using NUnit.Framework;
using Metasia.Core.Project;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Tests.Project
{
    [TestFixture]
    public class ProjectInfoTests
    {
        [Test]
        public void Constructor_SetsAllSpecifiedValues()
        {
            // Arrange
            int expectedFramerate = 60;
            var expectedSize = new SKSize(3840, 2160);
            int expectedAudioSamplingRate = 48000;
            int expectedAudioChannels = 2;

            // Act
            var projectInfo = new ProjectInfo(expectedFramerate, expectedSize, expectedAudioSamplingRate, expectedAudioChannels);

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(expectedFramerate));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(expectedSize.Width));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(expectedSize.Height));
            Assert.That(projectInfo.AudioSamplingRate, Is.EqualTo(expectedAudioSamplingRate));
            Assert.That(projectInfo.AudioChannels, Is.EqualTo(expectedAudioChannels));
        }

        [Test]
        public void Properties_CanBeModified()
        {
            // Arrange
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);

            // Act
            projectInfo.Framerate = 24;
            projectInfo.Size = new SKSize(1280, 720);
            projectInfo.AudioSamplingRate = 48000;
            projectInfo.AudioChannels = 1;

            // Assert
            Assert.That(projectInfo.Framerate, Is.EqualTo(24));
            Assert.That(projectInfo.Size.Width, Is.EqualTo(1280));
            Assert.That(projectInfo.Size.Height, Is.EqualTo(720));
            Assert.That(projectInfo.AudioSamplingRate, Is.EqualTo(48000));
            Assert.That(projectInfo.AudioChannels, Is.EqualTo(1));
            Assert.That(projectInfo.AudioChannelLayout, Is.EqualTo(AudioChannelLayout.Mono));
        }

        [TestCase(1, AudioChannelLayout.Mono)]
        [TestCase(2, AudioChannelLayout.Stereo)]
        [TestCase(6, AudioChannelLayout.Surround5_1)]
        public void Constructor_SetsLayoutFromChannelCount(int channelCount, AudioChannelLayout expected)
        {
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, channelCount);

            Assert.That(projectInfo.AudioChannelLayout, Is.EqualTo(expected));
            Assert.That(projectInfo.AudioChannels, Is.EqualTo(channelCount));
        }

        [Test]
        public void AudioChannelLayout_UpdatesChannelCount()
        {
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);

            projectInfo.AudioChannelLayout = AudioChannelLayout.Surround5_1;

            Assert.That(projectInfo.AudioChannels, Is.EqualTo(6));
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(8)]
        public void AudioChannels_ThrowsForUnsupportedCount(int channelCount)
        {
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => projectInfo.AudioChannels = channelCount);
        }

        [Test]
        public void AudioChannelLayout_ThrowsForUndefinedValue()
        {
            var projectInfo = new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => projectInfo.AudioChannelLayout = (AudioChannelLayout)4);
        }
    }
}
