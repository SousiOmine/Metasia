namespace Metasia.Core.Sounds;

public static class AudioChunkConverter
{
    public static IAudioChunk ConvertToFormat(IAudioChunk source, IAudioFormat targetFormat, long requiredLength)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targetFormat);

        if (requiredLength <= 0)
        {
            return new AudioChunk(targetFormat, 0);
        }

        var result = new AudioChunk(targetFormat, requiredLength);
        if (source.Length <= 0)
        {
            return result;
        }

        var sourceFormat = source.Format;

        for (long outputFrame = 0; outputFrame < requiredLength; outputFrame++)
        {
            double sourcePosition = outputFrame * (double)sourceFormat.SampleRate / targetFormat.SampleRate;
            if (sourcePosition >= source.Length)
            {
                break;
            }

            long sourceFrame = (long)Math.Floor(sourcePosition);
            long nextSourceFrame = Math.Min(sourceFrame + 1, source.Length - 1);
            double interpolation = sourcePosition - sourceFrame;

            for (int outputChannel = 0; outputChannel < targetFormat.ChannelCount; outputChannel++)
            {
                double current = GetChannelSample(source, sourceFrame, outputChannel, targetFormat);
                double next = GetChannelSample(source, nextSourceFrame, outputChannel, targetFormat);
                double sample = current + ((next - current) * interpolation);

                long outputIndex = (outputFrame * targetFormat.ChannelCount) + outputChannel;
                result.Samples[outputIndex] = Math.Clamp(sample, -1.0, 1.0);
            }
        }

        return result;
    }

    private static double GetChannelSample(IAudioChunk chunk, long frame, int targetChannel, IAudioFormat targetFormat)
    {
        var sourceFormat = chunk.Format;

        // If target is mono and source is multi-channel, perform mixdown (average all channels)
        if (targetFormat.ChannelCount == 1 && sourceFormat.ChannelCount > 1 && targetChannel == 0)
        {
            double sum = 0;
            for (int channel = 0; channel < sourceFormat.ChannelCount; channel++)
            {
                long index = (frame * sourceFormat.ChannelCount) + channel;
                sum += chunk.Samples[index];
            }

            return sum / sourceFormat.ChannelCount;
        }

        if (sourceFormat.ChannelCount > targetChannel)
        {
            long index = (frame * sourceFormat.ChannelCount) + targetChannel;
            return chunk.Samples[index];
        }

        if (sourceFormat.ChannelCount == 1)
        {
            long index = frame;
            return chunk.Samples[index];
        }

        long mappedIndex = (frame * sourceFormat.ChannelCount) + (targetChannel % sourceFormat.ChannelCount);
        return chunk.Samples[mappedIndex];
    }
}
