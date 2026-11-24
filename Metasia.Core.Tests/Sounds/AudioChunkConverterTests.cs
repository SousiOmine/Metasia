using NUnit.Framework;
using Metasia.Core.Sounds;

namespace Metasia.Core.Tests.Sounds
{
    /// <summary>
    /// チャンネル数が異なるチャンクの変換を検証する。
    /// 並び順は1chがFC、2chがFL,FR、6chがFL,FR,FC,LFE,BL,BR。
    /// </summary>
    [TestFixture]
    public class AudioChunkConverterTests
    {
        private const double Tolerance = 1e-9;
        private const double SurroundCoefficient = 0.7071067811865476;

        private static IAudioChunk CreateChunk(int channelCount, params double[] samples)
        {
            return new AudioChunk(new AudioFormat(48000, channelCount), samples);
        }

        private static IAudioChunk Convert(IAudioChunk source, int targetChannelCount)
        {
            return AudioChunkConverter.ConvertToFormat(
                source,
                new AudioFormat(source.Format.SampleRate, targetChannelCount),
                source.Length);
        }

        [Test]
        public void MonoToStereo_CopiesSampleToBothChannels()
        {
            var source = CreateChunk(1, 0.5);

            var result = Convert(source, 2);

            Assert.That(result.Format.ChannelCount, Is.EqualTo(2));
            Assert.That(result.Samples[0], Is.EqualTo(0.5).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(0.5).Within(Tolerance));
        }

        [Test]
        public void MonoToSurround5_1_PlacesSampleOnFrontChannelsOnly()
        {
            var source = CreateChunk(1, 0.5);

            var result = Convert(source, 6);

            Assert.That(result.Samples[0], Is.EqualTo(0.5).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(0.5).Within(Tolerance));
            for (int channel = 2; channel < 6; channel++)
            {
                Assert.That(result.Samples[channel], Is.EqualTo(0.0).Within(Tolerance), $"チャンネル{channel}は無音であるべきです");
            }
        }

        [Test]
        public void StereoToMono_AveragesBothChannels()
        {
            var source = CreateChunk(2, 0.3, -0.1);

            var result = Convert(source, 1);

            Assert.That(result.Samples[0], Is.EqualTo(0.1).Within(Tolerance));
        }

        [Test]
        public void StereoToSurround5_1_PlacesSamplesOnFrontChannelsOnly()
        {
            var source = CreateChunk(2, 0.3, -0.4);

            var result = Convert(source, 6);

            Assert.That(result.Samples[0], Is.EqualTo(0.3).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(-0.4).Within(Tolerance));
            for (int channel = 2; channel < 6; channel++)
            {
                Assert.That(result.Samples[channel], Is.EqualTo(0.0).Within(Tolerance), $"チャンネル{channel}は無音であるべきです");
            }
        }

        [Test]
        public void Surround5_1ToStereo_MixesCenterAndSurroundAndDropsLfe()
        {
            // FL, FR, FC, LFE, BL, BR
            var source = CreateChunk(6, 0.1, 0.2, 0.4, 1.0, 0.2, 0.1);

            var result = Convert(source, 2);

            double expectedLeft = 0.1 + (SurroundCoefficient * 0.4) + (SurroundCoefficient * 0.2);
            double expectedRight = 0.2 + (SurroundCoefficient * 0.4) + (SurroundCoefficient * 0.1);
            Assert.That(result.Samples[0], Is.EqualTo(expectedLeft).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(expectedRight).Within(Tolerance));
        }

        [Test]
        public void Surround5_1ToMono_DropsLfeAndWeightsCenterAndSurround()
        {
            var source = CreateChunk(6, 0.1, 0.2, 0.4, 1.0, 0.2, 0.1);

            var result = Convert(source, 1);

            double expected = (0.1 + 0.2) / 2.0
                + (SurroundCoefficient * (0.4 + 0.2 + 0.1) / 2.0);
            Assert.That(result.Samples[0], Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void Surround5_1ToStereo_ClampsWhenSumExceedsFullScale()
        {
            var source = CreateChunk(6, 1.0, 1.0, 1.0, 0.0, 1.0, 1.0);

            var result = Convert(source, 2);

            Assert.That(result.Samples[0], Is.EqualTo(1.0).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(1.0).Within(Tolerance));
        }

        [Test]
        public void SameChannelCount_CopiesSamples()
        {
            var source = CreateChunk(6, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6);

            var result = Convert(source, 6);

            for (int channel = 0; channel < 6; channel++)
            {
                Assert.That(result.Samples[channel], Is.EqualTo(0.1 * (channel + 1)).Within(Tolerance));
            }
        }

        [Test]
        public void UnsupportedChannelCount_KeepsIndexBasedMapping()
        {
            // 4chは対応外のため、先頭2chをそのまま取り出す
            var source = CreateChunk(4, 0.1, 0.2, 0.3, 0.4);

            var result = Convert(source, 2);

            Assert.That(result.Samples[0], Is.EqualTo(0.1).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(0.2).Within(Tolerance));
        }

        [Test]
        public void Resampling_ProducesRequestedLength()
        {
            var source = new AudioChunk(new AudioFormat(48000, 2), new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8 });

            var result = AudioChunkConverter.ConvertToFormat(source, new AudioFormat(24000, 2), 2);

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.Samples[0], Is.EqualTo(0.1).Within(Tolerance));
            Assert.That(result.Samples[1], Is.EqualTo(0.2).Within(Tolerance));
            Assert.That(result.Samples[2], Is.EqualTo(0.5).Within(Tolerance));
            Assert.That(result.Samples[3], Is.EqualTo(0.6).Within(Tolerance));
        }
    }
}
