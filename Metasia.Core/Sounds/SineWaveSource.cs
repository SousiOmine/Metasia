


using System;
using System.Buffers;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Generates a sine wave audio source
    /// </summary>
    public class SineWaveSource : IAudioSource
    {
        private double _frequency;
        private double _amplitude;

        /// <summary>
        /// Creates a new sine wave source
        /// </summary>
        /// <param name="frequency">Frequency in Hz</param>
        /// <param name="amplitude">Amplitude (0.0 to 1.0)</param>
        public SineWaveSource(double frequency, double amplitude)
        {
            _frequency = frequency;
            _amplitude = amplitude;
        }

        /// <summary>
        /// Gets an audio frame for the specified time
        /// </summary>
        public AudioFrame GetAudioFrame(byte channelCount, uint sampleRate, ushort fps, int frameIndex)
        {
            int sampleCount = channelCount * (int)(sampleRate / fps);
            double[] samples = ArrayPool<double>.Shared.Rent(sampleCount);

            try
            {
                double t = 0;
                double increment = 2 * Math.PI * _frequency / sampleRate;

                for (int i = 0; i < sampleCount; i++)
                {
                    samples[i] = _amplitude * Math.Sin(t);
                    t += increment;
                }

                return new AudioFrame(channelCount, sampleRate, fps, samples);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(samples);
            }
        }
    }
}


