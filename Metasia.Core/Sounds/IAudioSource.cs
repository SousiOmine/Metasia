
using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Interface for audio sources that can generate audio frames
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// Gets an audio frame for a specific time range
        /// </summary>
        /// <param name="channelCount">Number of audio channels</param>
        /// <param name="sampleRate">Sample rate in Hz</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="frame">The frame number to generate audio for</param>
        /// <returns>An audio frame containing the generated audio data</returns>
        AudioFrame GetAudioFrame(byte channelCount, uint sampleRate, ushort fps, int frame);
    }
}
