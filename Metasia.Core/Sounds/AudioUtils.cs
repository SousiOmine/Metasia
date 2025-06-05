


using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Utility methods for audio processing
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Mixes multiple audio frames into a single audio frame
        /// </summary>
        /// <param name="channel">Number of channels in the output</param>
        /// <param name="sounds">Audio frames to mix</param>
        /// <returns>A new AudioFrame containing the mixed audio</returns>
        public static AudioFrame Mix(byte channel, params AudioFrame[] sounds)
        {
            if (sounds.Length == 0) return new SilenceSource().GenerateAudioFrame(channel, sounds[0].SampleRate, sounds[0].FPS, 0);

            int samplesPerFrame = (int)(channel * (sounds[0].SampleRate / (double)sounds[0].FPS));
            double[] pulse = new double[samplesPerFrame];

            foreach (var sound in sounds)
            {
                for (int i = 0; i < pulse.Length; i++)
                {
                    pulse[i] += sound.Pulse.Span[i];
                }
            }

            return new AudioFrame(pulse, channel, sounds[0].SampleRate, sounds[0].FPS);
        }

        /// <summary>
        /// Adjusts the volume of an audio frame
        /// </summary>
        /// <param name="sound">The audio frame to adjust</param>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        /// <returns>A new AudioFrame with adjusted volume</returns>
        public static AudioFrame ChangeVolume(AudioFrame sound, double volume)
        {
            double[] newPulse = new double[sound.Pulse.Length];
            for (int i = 0; i < sound.Pulse.Length; i++)
            {
                newPulse[i] = sound.Pulse.Span[i] * volume;
            }

            return sound.WithSamples(newPulse);
        }
    }
}

