using NUnit.Framework;
using Metasia.Core.Sounds;

namespace Metasia.Core.Tests.Sounds
{
    [TestFixture]
    public class AudioFormatTests
    {
        [Test]
        public void Constructor_WithValidStandardParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            int sampleRate = 44100;
            int channelCount = 2;

            // Act
            var audioFormat = new AudioFormat(sampleRate, channelCount);

            // Assert
            Assert.That(audioFormat.SampleRate, Is.EqualTo(sampleRate));
            Assert.That(audioFormat.ChannelCount, Is.EqualTo(channelCount));
        }

        [Test]
        public void Constructor_WithValidMonoParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            int sampleRate = 48000;
            int channelCount = 1;

            // Act
            var audioFormat = new AudioFormat(sampleRate, channelCount);

            // Assert
            Assert.That(audioFormat.SampleRate, Is.EqualTo(sampleRate));
            Assert.That(audioFormat.ChannelCount, Is.EqualTo(channelCount));
        }

        [Test]
        public void Constructor_WithHighSampleRate_SetsPropertiesCorrectly()
        {
            // Arrange
            int sampleRate = 192000;
            int channelCount = 8;

            // Act
            var audioFormat = new AudioFormat(sampleRate, channelCount);

            // Assert
            Assert.That(audioFormat.SampleRate, Is.EqualTo(sampleRate));
            Assert.That(audioFormat.ChannelCount, Is.EqualTo(channelCount));
        }

        [Test]
        public void Constructor_WithMinimumValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            int sampleRate = 1;
            int channelCount = 1;

            // Act
            var audioFormat = new AudioFormat(sampleRate, channelCount);

            // Assert
            Assert.That(audioFormat.SampleRate, Is.EqualTo(sampleRate));
            Assert.That(audioFormat.ChannelCount, Is.EqualTo(channelCount));
        }

        [Test]
        public void Constructor_WithZeroSampleRate_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = 0;
            int channelCount = 2;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithNegativeSampleRate_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = -1;
            int channelCount = 2;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithIntMinSampleRate_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = int.MinValue;
            int channelCount = 2;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithZeroChannelCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = 44100;
            int channelCount = 0;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithNegativeChannelCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = 44100;
            int channelCount = -1;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithIntMinChannelCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = 44100;
            int channelCount = int.MinValue;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithBothParametersInvalid_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = -1;
            int channelCount = 0;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void Constructor_WithBothParametersNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int sampleRate = -100;
            int channelCount = -5;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioFormat(sampleRate, channelCount));
            Assert.That(ex.Message, Does.Contain("SampleRate and ChannelCount must be positive"));
        }

        [Test]
        public void SampleRate_Property_ReturnsCorrectValue()
        {
            // Arrange
            int expectedSampleRate = 96000;
            var audioFormat = new AudioFormat(expectedSampleRate, 2);

            // Act & Assert
            Assert.That(audioFormat.SampleRate, Is.EqualTo(expectedSampleRate));
        }

        [Test]
        public void ChannelCount_Property_ReturnsCorrectValue()
        {
            // Arrange
            int expectedChannelCount = 4;
            var audioFormat = new AudioFormat(44100, expectedChannelCount);

            // Act & Assert
            Assert.That(audioFormat.ChannelCount, Is.EqualTo(expectedChannelCount));
        }

        [Test]
        public void SampleRate_Property_IsReadOnly()
        {
            // Arrange
            var audioFormat = new AudioFormat(44100, 2);

            // Act & Assert
            // プロパティが読み取り専用であることを確認するテスト
            // C#では読み取り専用プロパティへの代入はコンパイルエラーになるため、
            // リフレクションを使用して設定不可能であることを確認
            var propertyInfo = typeof(AudioFormat).GetProperty("SampleRate");
            Assert.That(propertyInfo.CanWrite, Is.False);
        }

        [Test]
        public void ChannelCount_Property_IsReadOnly()
        {
            // Arrange
            var audioFormat = new AudioFormat(44100, 2);

            // Act & Assert
            // プロパティが読み取り専用であることを確認するテスト
            var propertyInfo = typeof(AudioFormat).GetProperty("ChannelCount");
            Assert.That(propertyInfo.CanWrite, Is.False);
        }
    }
}
