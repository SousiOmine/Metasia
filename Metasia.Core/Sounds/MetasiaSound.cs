

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
        /// Creates a new MetasiaSound
        /// </summary>
        public MetasiaSound(byte Channel, uint SampleRate, ushort FPS)
        {
            Pulse = new double[Channel * (SampleRate / FPS)];
            _channel = Channel;
            _sampleRate = SampleRate;
            _fps = FPS;
        }

        /// <summary>
        /// Creates a MetasiaSound from an AudioFrame
        /// </summary>
        public MetasiaSound(AudioFrame audioFrame)
        {
            _channel = audioFrame.ChannelCount;
            _sampleRate = audioFrame.SampleRate;
            _fps = audioFrame.FPS;
            Pulse = new double[audioFrame.SampleCount];
            audioFrame.Samples.Span.CopyTo(Pulse);
        }

        /// <summary>
        /// Disposes the MetasiaSound
        /// </summary>
        public void Dispose()
        {
            Pulse = null;
        }

        /// <summary>
        /// Mixes multiple MetasiaSound objects
        /// </summary>
        public static MetasiaSound SynthesisPulse(byte channel, params MetasiaSound[] sounds)
        {
            MetasiaSound result = new MetasiaSound(channel, sounds[0].SampleRate, sounds[0].FPS);
            for(int i = 0; i < result.Pulse.Length; i++)
            {
                for(int j = 0; j < sounds.Length; j++)
                {
                    result.Pulse[i] += sounds[j].Pulse[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Changes the volume of a MetasiaSound
        /// </summary>
        public static MetasiaSound VolumeChange(MetasiaSound sound, double volume)
        {
            for(int i = 0; i < sound.Pulse.Length; i++)
            {
                sound.Pulse[i] *= volume;
            }
            return sound;
        }
    }
}

