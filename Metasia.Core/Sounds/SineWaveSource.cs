

using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Generates a sine wave audio signal
    /// </summary>
    public class SineWaveSource : IAudioSource
    {
        private readonly double _frequency;
        private readonly double _amplitude;

        /// <summary>
        /// Creates a new sine wave source
        /// </summary>
        /// <param name="frequency">Frequency of the sine wave in Hz</param>
        /// <param name="amplitude">Amplitude of the sine wave (0.0 to 1.0)</param>
        public SineWaveSource(double frequency = 440.0, double amplitude = 0.5)
        {
            _frequency = frequency;
            _amplitude = amplitude;
        }

        public AudioFrame GenerateAudioFrame(byte channel, uint sampleRate, ushort fps, int frame)
        {
            int samplesPerFrame = (int)(channel * (sampleRate / (double)fps));
            double[] pulse = new double[samplesPerFrame];

            int audioOffset = frame * pulse.Length;

            for (int i = 0; i < pulse.Length; i += channel)
            {
                double sampleTime = (i + audioOffset) / (double)sampleRate;
                double value = Math.Sin(2.0 * Math.PI * _frequency * sampleTime) * _amplitude;

                for (int c = 0; c < channel; c++)
                {
                    pulse[i + c] = value;
                }
            }

            return new AudioFrame(pulse, channel, sampleRate, fps);
        }
    }
}

