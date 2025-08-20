namespace Metasia.Core.Sounds
{
    public class AudioChunk
    {
        public double[] Samples { get; }

        public AudioFormat Format { get; }

        public long Length => (long)Samples.Length / Format.ChannelCount;

        public AudioChunk(AudioFormat format, long length)
        {
            Format = format;
            Samples = new double[length * format.ChannelCount];
        }

        public AudioChunk(AudioFormat format, double[] samples)
        {
            if (samples.Length % format.ChannelCount != 0)
            {
                throw new ArgumentException("The length of samples must be a multiple of the channel count");
            }
            Format = format;
            Samples = samples;
        }
        
    }
}