
using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Represents an immutable audio frame containing PCM samples
    /// </summary>
    public class AudioFrame
    {
        /// <summary>
        /// The PCM samples for this audio frame
        /// </summary>
        public ReadOnlyMemory<double> Pulse { get; }

        /// <summary>
        /// Number of channels in the audio
        /// </summary>
        public byte Channel { get; }

        /// <summary>
        /// Sample rate of the audio
        /// </summary>
        public uint SampleRate { get; }

        /// <summary>
        /// Frames per second
        /// </summary>
        public ushort FPS { get; }

        /// <summary>
        /// Creates a new audio frame
        /// </summary>
        /// <param name="pulse">The PCM samples</param>
        /// <param name="channel">Number of channels</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fps">Frames per second</param>
        public AudioFrame(double[] pulse, byte channel, uint sampleRate, ushort fps)
        {
            Pulse = new ReadOnlyMemory<double>(pulse);
            Channel = channel;
            SampleRate = sampleRate;
            FPS = fps;
        }

        /// <summary>
        /// Creates a new audio frame with the same properties but different sample data
        /// </summary>
        /// <param name="pulse">The new PCM samples</param>
        /// <returns>A new AudioFrame with the specified samples</returns>
        public AudioFrame WithSamples(double[] pulse)
        {
            return new AudioFrame(pulse, Channel, SampleRate, FPS);
        }
    }
}
