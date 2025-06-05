
using System;
using System.Linq;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Legacy audio class for backward compatibility
    /// </summary>
    public class MetasiaSound : IDisposable
    {
        public double[] Pulse;
        private byte _channel;
        private uint _sampleRate;
        private ushort _fps;

        /// <summary>
        /// 音声のチャンネル数
        /// </summary>
        public byte Channel
        {
            get => _channel;
        }

        /// <summary>
        /// 音声のサンプリングレート
        /// </summary>
        public uint SampleRate
        {
            get => _sampleRate;
            set
            {
                _sampleRate = value;
                Pulse = new double[_channel * (_sampleRate / FPS)];
            }
        }

        /// <summary>
        /// 一秒間に何フレームで構成されるか
        /// </summary>
        public ushort FPS
        {
            get => _fps;
            set
            {
                _fps = value;
                Pulse = new double[_channel * (_sampleRate / FPS)];
            }
        }

        /// <summary>
        /// Creates a new MetasiaSound from an AudioFrame
        /// </summary>
        /// <param name="audioFrame">The AudioFrame to convert</param>
        /// <param name="channel">Number of channels</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fps">Frames per second</param>
        public MetasiaSound(AudioFrame audioFrame)
        {
            Pulse = audioFrame.Pulse.ToArray();
            _channel = audioFrame.Channel;
            _sampleRate = audioFrame.SampleRate;
            _fps = audioFrame.FPS;
        }

        /// <summary>
        /// Creates a new MetasiaSound with the specified parameters
        /// </summary>
        /// <param name="channel">Number of channels</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fps">Frames per second</param>
        public MetasiaSound(byte channel, uint sampleRate, ushort fps)
        {
            Pulse = new double[channel * (sampleRate / fps)];
            _channel = channel;
            _sampleRate = sampleRate;
            _fps = fps;
        }

        /// <summary>
        /// Creates a new MetasiaSound with the specified sample data
        /// </summary>
        /// <param name="pulse">Sample data</param>
        /// <param name="channel">Number of channels</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="fps">Frames per second</param>
        public MetasiaSound(double[] pulse, byte channel, uint sampleRate, ushort fps)
        {
            Pulse = pulse;
            _channel = channel;
            _sampleRate = sampleRate;
            _fps = fps;
        }

        public void Dispose()
        {
            Pulse = null;
        }

        /// <summary>
        /// Mixes multiple audio frames into a single audio frame
        /// </summary>
        /// <param name="channel">Number of channels in the output</param>
        /// <param name="sounds">Audio frames to mix</param>
        /// <returns>A new MetasiaSound containing the mixed audio</returns>
        public static MetasiaSound SynthesisPulse(byte channel, params MetasiaSound[] sounds)
        {
            return new MetasiaSound(AudioUtils.Mix(channel, sounds.Select(s => new AudioFrame(s.Pulse, s.Channel, s.SampleRate, s.FPS)).ToArray()));
        }

        /// <summary>
        /// Adjusts the volume of an audio frame
        /// </summary>
        /// <param name="sound">The audio frame to adjust</param>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        /// <returns>A new MetasiaSound with adjusted volume</returns>
        public static MetasiaSound VolumeChange(MetasiaSound sound, double volume)
        {
            return new MetasiaSound(AudioUtils.ChangeVolume(new AudioFrame(sound.Pulse, sound.Channel, sound.SampleRate, sound.FPS), volume));
        }
    }
}
