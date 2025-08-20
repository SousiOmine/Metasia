namespace Metasia.Core.Sounds
{
    public class AudioFormat
    {
        public int SampleRate { get => _sampleRate; init => _sampleRate = value; }
        public int ChannelCount { get => _channelCount; init => _channelCount = value; }

        private int _sampleRate = 44100;
        private int _channelCount = 2;

        public AudioFormat(int sampleRate, int channelCount)
        {
            if (sampleRate <= 0 || channelCount <= 0)
            {
                throw new ArgumentOutOfRangeException("SampleRate and ChannelCount must be positive");
            }
            _sampleRate = sampleRate;
            _channelCount = channelCount;
        }
    }
}