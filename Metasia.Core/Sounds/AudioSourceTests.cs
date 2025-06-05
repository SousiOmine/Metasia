

using System;
using System.Linq;

namespace Metasia.Core.Sounds.Tests
{
    public static class AudioSourceTests
    {
        public static void RunTests()
        {
            Console.WriteLine("Running audio source tests...");

            // Test silence source
            TestSilenceSource();

            // Test sine wave source
            TestSineWaveSource();

            // Test audio mixing
            TestAudioMixing();

            // Test volume change
            TestVolumeChange();

            Console.WriteLine("All tests passed!");
        }

        private static void TestSilenceSource()
        {
            IAudioSource silenceSource = new SilenceSource();
            AudioFrame audioFrame = silenceSource.GenerateAudioFrame(2, 44100, 60, 0);

            // Check that all samples are 0
            bool isSilent = true;
            foreach (var sample in audioFrame.Pulse.Span)
            {
                if (sample != 0)
                {
                    isSilent = false;
                    break;
                }
            }

            if (!isSilent)
            {
                throw new Exception("Silence source generated non-zero samples");
            }

            Console.WriteLine("Silence source test passed");
        }

        private static void TestSineWaveSource()
        {
            IAudioSource sineSource = new SineWaveSource(440, 0.5);
            AudioFrame audioFrame = sineSource.GenerateAudioFrame(2, 44100, 60, 0);

            // Check that samples are not all 0
            bool hasSignal = false;
            foreach (var sample in audioFrame.Pulse.Span)
            {
                if (sample != 0)
                {
                    hasSignal = true;
                    break;
                }
            }

            if (!hasSignal)
            {
                throw new Exception("Sine wave source generated no signal");
            }

            Console.WriteLine("Sine wave source test passed");
        }

        private static void TestAudioMixing()
        {
            IAudioSource silenceSource = new SilenceSource();
            IAudioSource sineSource = new SineWaveSource(440, 0.5);

            AudioFrame silence = silenceSource.GenerateAudioFrame(2, 44100, 60, 0);
            AudioFrame sine = sineSource.GenerateAudioFrame(2, 44100, 60, 0);

            AudioFrame mixed = AudioUtils.Mix(2, silence, sine);

            // Check that mixed audio is not silent
            bool hasSignal = false;
            foreach (var sample in mixed.Pulse.Span)
            {
                if (sample != 0)
                {
                    hasSignal = true;
                    break;
                }
            }

            if (!hasSignal)
            {
                throw new Exception("Mixed audio has no signal");
            }

            Console.WriteLine("Audio mixing test passed");
        }

        private static void TestVolumeChange()
        {
            IAudioSource sineSource = new SineWaveSource(440, 0.5);
            AudioFrame audioFrame = sineSource.GenerateAudioFrame(2, 44100, 60, 0);

            // Adjust volume to 0.5
            AudioFrame adjusted = AudioUtils.ChangeVolume(audioFrame, 0.5);

            // Check that all samples are halved
            bool isHalved = true;
            for (int i = 0; i < audioFrame.Pulse.Length; i++)
            {
                if (Math.Abs(audioFrame.Pulse.Span[i] - (adjusted.Pulse.Span[i] * 2)) >= 0.0001)
                {
                    isHalved = false;
                    break;
                }
            }

            if (!isHalved)
            {
                throw new Exception("Volume adjustment failed");
            }

            Console.WriteLine("Volume change test passed");
        }
    }
}

