using NUnit.Framework;
using Metasia.Core.Sounds;
using Moq;

namespace Metasia.Core.Tests.Sounds
{
    [TestFixture]
    public class AudioChunkTests
    {
        private AudioFormat _stereoFormat;
        private AudioFormat _monoFormat;

        [SetUp]
        public void Setup()
        {
            _stereoFormat = new AudioFormat(44100, 2);
            _monoFormat = new AudioFormat(48000, 1);
        }

        [Test]
        public void Constructor_WithFormatAndLength_CreatesCorrectSamplesArray()
        {
            // Arrange
            var format = new AudioFormat(44100, 2);
            long length = 100;

            // Act
            var audioChunk = new AudioChunk(format, length);

            // Assert
            Assert.That(audioChunk.Format, Is.EqualTo(format));
            Assert.That(audioChunk.Samples, Is.Not.Null);
            Assert.That(audioChunk.Samples.Length, Is.EqualTo(length * format.ChannelCount));
            Assert.That(audioChunk.Length, Is.EqualTo(length));
        }

        [Test]
        public void Constructor_WithFormatAndSamples_InitializesCorrectly()
        {
            // Arrange
            var format = new AudioFormat(44100, 2);
            var samples = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6 }; // 3 samples, 2 channels

            // Act
            var audioChunk = new AudioChunk(format, samples);

            // Assert
            Assert.That(audioChunk.Format, Is.EqualTo(format));
            Assert.That(audioChunk.Samples, Is.SameAs(samples));
            Assert.That(audioChunk.Length, Is.EqualTo(3)); // 6 samples / 2 channels = 3 length
        }

        [Test]
        public void Constructor_WithFormatAndSamples_NullFormat_ThrowsArgumentNullException()
        {
            // Arrange
            double[] samples = new double[] { 0.1, 0.2 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AudioChunk(null, samples));
        }

        [Test]
        public void Constructor_WithFormatAndSamples_NullSamples_ThrowsArgumentNullException()
        {
            // Arrange
            var format = new AudioFormat(44100, 2);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AudioChunk(format, null));
        }

        [Test]
        public void Constructor_WithFormatAndSamples_ZeroChannelCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            // IAudioFormatをモック化して、ChannelCountプロパティが0を返すように設定
            var mockFormat = new Mock<IAudioFormat>();
            mockFormat.Setup(f => f.SampleRate).Returns(44100);
            mockFormat.Setup(f => f.ChannelCount).Returns(0);
            
            var samples = new double[] { 0.1, 0.2 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new AudioChunk(mockFormat.Object, samples));
            Assert.That(ex.Message, Does.Contain("format.ChannelCount must be greater than zero"));
        }

        [Test]
        public void Constructor_WithFormatAndSamples_SamplesLengthNotMultipleOfChannelCount_ThrowsArgumentException()
        {
            // Arrange
            var format = new AudioFormat(44100, 2); // 2 channels
            var samples = new double[] { 0.1, 0.2, 0.3 }; // 3 samples (not multiple of 2)

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new AudioChunk(format, samples));
            Assert.That(ex.Message, Does.Contain("The length of samples must be a multiple of the channel count"));
        }

        [Test]
        public void Length_Property_ReturnsCorrectValueForStereo()
        {
            // Arrange
            var samples = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8 }; // 8 samples
            var audioChunk = new AudioChunk(_stereoFormat, samples); // 2 channels

            // Act & Assert
            Assert.That(audioChunk.Length, Is.EqualTo(4)); // 8 samples / 2 channels = 4 length
        }

        [Test]
        public void Length_Property_ReturnsCorrectValueForMono()
        {
            // Arrange
            var samples = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 }; // 5 samples
            var audioChunk = new AudioChunk(_monoFormat, samples); // 1 channel

            // Act & Assert
            Assert.That(audioChunk.Length, Is.EqualTo(5)); // 5 samples / 1 channel = 5 length
        }

        [Test]
        public void Constructor_WithFormatAndLength_ZeroLength_CreatesEmptyArray()
        {
            // Arrange
            var format = new AudioFormat(44100, 2);
            long length = 0;

            // Act
            var audioChunk = new AudioChunk(format, length);

            // Assert
            Assert.That(audioChunk.Samples, Is.Not.Null);
            Assert.That(audioChunk.Samples.Length, Is.EqualTo(0));
            Assert.That(audioChunk.Length, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithFormatAndSamples_EmptySamplesArray_WorksCorrectly()
        {
            // Arrange
            var format = new AudioFormat(44100, 2);
            var samples = new double[0];

            // Act
            var audioChunk = new AudioChunk(format, samples);

            // Assert
            Assert.That(audioChunk.Samples, Is.SameAs(samples));
            Assert.That(audioChunk.Samples.Length, Is.EqualTo(0));
            Assert.That(audioChunk.Length, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithFormatAndLength_SingleChannel_CreatesCorrectArray()
        {
            // Arrange
            var format = new AudioFormat(48000, 1);
            long length = 10;

            // Act
            var audioChunk = new AudioChunk(format, length);

            // Assert
            Assert.That(audioChunk.Format, Is.EqualTo(format));
            Assert.That(audioChunk.Samples.Length, Is.EqualTo(length * format.ChannelCount));
            Assert.That(audioChunk.Samples.Length, Is.EqualTo(10));
            Assert.That(audioChunk.Length, Is.EqualTo(10));
        }
    }
}
