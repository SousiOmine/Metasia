
using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Interface for audio sources that can generate audio frames
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// Generates an audio frame for the specified time range
        /// </summary>
        /// <param name="channel">Number of channels</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="frame">The frame number to generate audio for</param>
        /// <returns>An AudioFrame containing the generated audio data</returns>
        AudioFrame GenerateAudioFrame(byte channel, uint sampleRate, ushort fps, int frame);
    }
}
