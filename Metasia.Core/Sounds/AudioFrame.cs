

using System;
using System.Linq;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Represents an immutable block of PCM audio data for a specific time duration
    /// </summary>
    public class AudioFrame : IDisposable
    {
        /// <summary>
        /// The PCM samples for this audio frame
        /// </summary>
        public ReadOnlyMemory<double> Samples { get; private init; }

        /// <summary>
        /// Number of channels in the audio
        /// </summary>
        public byte ChannelCount { get; }

        /// <summary>
        /// Sample rate of the audio
        /// </summary>
        public uint SampleRate { get; }

        /// <summary>
        /// Frames per second
        /// </summary>
        public ushort FPS { get; }

        /// <summary>
        /// Duration of this audio frame in seconds
        /// </summary>
        public double Duration => 1.0 / FPS;

        /// <summary>
        /// Number of samples in this frame
        /// </summary>
        public int SampleCount => Samples.Length;

        /// <summary>
        /// Creates a new audio frame
        /// </summary>
        /// <param name="channelCount">Number of channels (e.g., 1 for mono, 2 for stereo)</param>
        /// <param name="sampleRate">Sample rate in Hz</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="samples">PCM samples for this frame</param>
        public AudioFrame(byte channelCount, uint sampleRate, ushort fps, double[] samples)
        {
            ChannelCount = channelCount;
            SampleRate = sampleRate;
            FPS = fps;
            Samples = new ReadOnlyMemory<double>(samples);
        }

        /// <summary>
        /// Creates a new audio frame with silence
        /// </summary>
        public static AudioFrame CreateSilence(byte channelCount, uint sampleRate, ushort fps)
        {
            int sampleCount = channelCount * (int)(sampleRate / fps);
            double[] samples = new double[sampleCount];
            return new AudioFrame(channelCount, sampleRate, fps, samples);
        }

        /// <summary>
        /// Mixes multiple audio frames together
        /// </summary>
        public static AudioFrame Mix(params AudioFrame[] frames)
        {
            if (frames.Length == 0) return null;

            byte channelCount = frames[0].ChannelCount;
            uint sampleRate = frames[0].SampleRate;
            ushort fps = frames[0].FPS;
            int sampleCount = frames[0].SampleCount;

            double[] resultSamples = new double[sampleCount];

            foreach (var frame in frames)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    resultSamples[i] += frame.Samples.Span[i];
                }
            }

            return new AudioFrame(channelCount, sampleRate, fps, resultSamples);
        }

        /// <summary>
        /// Adjusts the volume of an audio frame
        /// </summary>
        public static AudioFrame ChangeVolume(AudioFrame frame, double volume)
        {
            double[] newSamples = new double[frame.SampleCount];
            for (int i = 0; i < frame.SampleCount; i++)
            {
                newSamples[i] = frame.Samples.Span[i] * volume;
            }
            return new AudioFrame(frame.ChannelCount, frame.SampleRate, frame.FPS, newSamples);
        }

        /// <summary>
        /// Disposes the audio frame
        /// </summary>
        public void Dispose()
        {
            // In a real implementation, we might need to dispose unmanaged resources
            // For now, this is just a placeholder
        }
    }
}

