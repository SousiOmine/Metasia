



using Metasia.Core.Sounds;
using Xunit;

namespace Metasia.Core.Tests
{
    public class AudioSystemTests
    {
        [Fact]
        public void TestAudioFrameCreation()
        {
            // Test creating an audio frame
            byte channelCount = 2; // Stereo
            uint sampleRate = 44100;
            ushort fps = 60;

            // Create a silence audio frame
            AudioFrame silenceFrame = AudioFrame.CreateSilence(channelCount, sampleRate, fps);

            Assert.Equal(channelCount, silenceFrame.ChannelCount);
            Assert.Equal(sampleRate, silenceFrame.SampleRate);
            Assert.Equal(fps, silenceFrame.FPS);
            Assert.Equal(channelCount * (sampleRate / fps), silenceFrame.SampleCount);

            // Check that all samples are 0 (silence)
            foreach (var sample in silenceFrame.Samples.Span)
            {
                Assert.Equal(0.0, sample);
            }
        }

        [Fact]
        public void TestSineWaveSource()
        {
            // Test the sine wave source
            var sineWaveSource = new SineWaveSource(440.0, 0.5);
            byte channelCount = 2;
            uint sampleRate = 44100;
            ushort fps = 60;

            AudioFrame audioFrame = sineWaveSource.GetAudioFrame(channelCount, sampleRate, fps, 0);

            // Check that the audio frame has the correct properties
            Assert.Equal(channelCount, audioFrame.ChannelCount);
            Assert.Equal(sampleRate, audioFrame.SampleRate);
            Assert.Equal(fps, audioFrame.FPS);

            // Check that the samples are not all 0 (there should be audio data)
            bool hasAudio = false;
            foreach (var sample in audioFrame.Samples.Span)
            {
                if (sample != 0.0)
                {
                    hasAudio = true;
                    break;
                }
            }

            Assert.True(hasAudio);
        }

        [Fact]
        public void TestAudioMixing()
        {
            // Test mixing two audio frames
            byte channelCount = 2;
            uint sampleRate = 44100;
            ushort fps = 60;

            // Create a silence frame
            AudioFrame silenceFrame = AudioFrame.CreateSilence(channelCount, sampleRate, fps);

            // Create a sine wave frame
            var sineWaveSource = new SineWaveSource(440.0, 0.5);
            AudioFrame sineWaveFrame = sineWaveSource.GetAudioFrame(channelCount, sampleRate, fps, 0);

            // Mix the frames
            AudioFrame mixedFrame = AudioFrame.Mix(silenceFrame, sineWaveFrame);

            // Check that the mixed frame has the correct properties
            Assert.Equal(channelCount, mixedFrame.ChannelCount);
            Assert.Equal(sampleRate, mixedFrame.SampleRate);
            Assert.Equal(fps, mixedFrame.FPS);

            // Check that the samples are a combination of the two frames
            for (int i = 0; i < mixedFrame.SampleCount; i++)
            {
                Assert.Equal(sineWaveFrame.Samples.Span[i], mixedFrame.Samples.Span[i]);
            }
        }

        [Fact]
        public void TestVolumeChange()
        {
            // Test changing the volume of an audio frame
            byte channelCount = 2;
            uint sampleRate = 44100;
            ushort fps = 60;

            // Create a sine wave frame
            var sineWaveSource = new SineWaveSource(440.0, 0.5);
            AudioFrame originalFrame = sineWaveSource.GetAudioFrame(channelCount, sampleRate, fps, 0);

            // Change the volume to 0.5
            AudioFrame halfVolumeFrame = AudioUtils.ChangeVolume(originalFrame, 0.5);

            // Check that the samples are half the amplitude of the original
            for (int i = 0; i < originalFrame.SampleCount; i++)
            {
                Assert.Equal(originalFrame.Samples.Span[i] * 0.5, halfVolumeFrame.Samples.Span[i]);
            }
        }
    }
}


