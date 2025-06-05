

using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Generates silence (zero amplitude) audio
    /// </summary>
    public class SilenceSource : IAudioSource
    {
        public AudioFrame GenerateAudioFrame(byte channel, uint sampleRate, ushort fps, int frame)
        {
            int samplesPerFrame = (int)(channel * (sampleRate / (double)fps));
            double[] pulse = new double[samplesPerFrame];

            return new AudioFrame(pulse, channel, sampleRate, fps);
        }
    }
}

