

using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Generates silence audio
    /// </summary>
    public class SilenceSource : IAudioSource
    {
        /// <summary>
        /// Gets a silence audio frame
        /// </summary>
        public AudioFrame GetAudioFrame(byte channelCount, uint sampleRate, ushort fps, int frame)
        {
            return AudioFrame.CreateSilence(channelCount, sampleRate, fps);
        }
    }
}

