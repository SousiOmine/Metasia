using NUnit.Framework;
using Metasia.Core.Sounds;

namespace Metasia.Core.Tests.Sounds
{
    [TestFixture]
    public class AudioChannelLayoutTests
    {
        [TestCase(1, AudioChannelLayout.Mono)]
        [TestCase(2, AudioChannelLayout.Stereo)]
        [TestCase(6, AudioChannelLayout.Surround5_1)]
        public void TryFromChannelCount_ReturnsSupportedLayout(int channelCount, AudioChannelLayout expected)
        {
            bool result = AudioChannelLayouts.TryFromChannelCount(channelCount, out AudioChannelLayout layout);

            Assert.That(result, Is.True);
            Assert.That(layout, Is.EqualTo(expected));
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(8)]
        public void TryFromChannelCount_RejectsUnsupportedCount(int channelCount)
        {
            bool result = AudioChannelLayouts.TryFromChannelCount(channelCount, out _);

            Assert.That(result, Is.False);
        }

        [Test]
        public void FromChannelCount_ThrowsForUnsupportedCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AudioChannelLayouts.FromChannelCount(4));
        }

        [TestCase(AudioChannelLayout.Mono, "Mono")]
        [TestCase(AudioChannelLayout.Stereo, "Stereo")]
        [TestCase(AudioChannelLayout.Surround5_1, "Surround5_1")]
        public void ToName_ReturnsStableName(AudioChannelLayout layout, string expected)
        {
            Assert.That(AudioChannelLayouts.ToName(layout), Is.EqualTo(expected));
        }

        [TestCase("Mono", AudioChannelLayout.Mono)]
        [TestCase("Stereo", AudioChannelLayout.Stereo)]
        [TestCase("Surround5_1", AudioChannelLayout.Surround5_1)]
        [TestCase("surround5.1", AudioChannelLayout.Surround5_1)]
        [TestCase("5.1", AudioChannelLayout.Surround5_1)]
        public void TryParse_ParsesSavedName(string name, AudioChannelLayout expected)
        {
            bool result = AudioChannelLayouts.TryParse(name, out AudioChannelLayout layout);

            Assert.That(result, Is.True);
            Assert.That(layout, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Unknown")]
        public void TryParse_RejectsUnknownName(string? name)
        {
            Assert.That(AudioChannelLayouts.TryParse(name, out _), Is.False);
        }

        [Test]
        public void ChannelCount_MatchesEnumValue()
        {
            Assert.That((int)AudioChannelLayout.Mono, Is.EqualTo(1));
            Assert.That((int)AudioChannelLayout.Stereo, Is.EqualTo(2));
            Assert.That((int)AudioChannelLayout.Surround5_1, Is.EqualTo(6));
        }
    }
}
