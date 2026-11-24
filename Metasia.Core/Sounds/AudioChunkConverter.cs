namespace Metasia.Core.Sounds;

public static class AudioChunkConverter
{
    /// <summary>
    /// 5.1chのセンターとサラウンドをステレオへ混ぜるときの係数。ITU-R BS.775に合わせて-3dBとする
    /// </summary>
    private const double SurroundMixCoefficient = 0.7071067811865476;

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
                double current = GetChannelSample(source, sourceFrame, outputChannel, targetFormat.ChannelCount);
                double next = GetChannelSample(source, nextSourceFrame, outputChannel, targetFormat.ChannelCount);
                double sample = current + ((next - current) * interpolation);

                long outputIndex = (outputFrame * targetFormat.ChannelCount) + outputChannel;
                result.Samples[outputIndex] = Math.Clamp(sample, -1.0, 1.0);
            }
        }

        return result;
    }

    private static double GetChannelSample(IAudioChunk chunk, long frame, int targetChannel, int targetChannelCount)
    {
        var sourceFormat = chunk.Format;
        int sourceChannelCount = sourceFormat.ChannelCount;

        if (sourceChannelCount == targetChannelCount)
        {
            long index = (frame * sourceChannelCount) + targetChannel;
            return chunk.Samples[index];
        }

        if (AudioChannelLayouts.TryFromChannelCount(sourceChannelCount, out AudioChannelLayout sourceLayout)
            && AudioChannelLayouts.TryFromChannelCount(targetChannelCount, out AudioChannelLayout targetLayout))
        {
            return MixChannel(chunk.Samples, frame, sourceLayout, targetLayout, targetChannel);
        }

        return GetChannelSampleFallback(chunk, frame, targetChannel, targetChannelCount);
    }

    /// <summary>
    /// 対応しているチャンネル数同士の変換。並び順はWAVEFORMATEXTENSIBLE準拠で固定されている
    /// </summary>
    private static double MixChannel(
        double[] samples,
        long frame,
        AudioChannelLayout sourceLayout,
        AudioChannelLayout targetLayout,
        int targetChannel)
    {
        int sourceChannelCount = (int)sourceLayout;
        long frameOffset = frame * sourceChannelCount;

        return (sourceLayout, targetLayout) switch
        {
            // モノラルはFLとFRの両方へ配置する
            (AudioChannelLayout.Mono, AudioChannelLayout.Stereo) => samples[frameOffset],
            (AudioChannelLayout.Mono, AudioChannelLayout.Surround5_1) =>
                targetChannel <= 1 ? samples[frameOffset] : 0.0,

            // ステレオはFLとFRへ配置し、センターとサラウンドには置かない
            (AudioChannelLayout.Stereo, AudioChannelLayout.Mono) =>
                (samples[frameOffset] + samples[frameOffset + 1]) * 0.5,
            (AudioChannelLayout.Stereo, AudioChannelLayout.Surround5_1) =>
                targetChannel <= 1 ? samples[frameOffset + targetChannel] : 0.0,

            // 5.1はセンターとサラウンドを-3dBで混ぜ、LFEは落とす
            (AudioChannelLayout.Surround5_1, AudioChannelLayout.Mono) =>
                MixSurroundToMono(samples, frameOffset),
            (AudioChannelLayout.Surround5_1, AudioChannelLayout.Stereo) =>
                MixSurroundToStereo(samples, frameOffset, targetChannel),

            _ => throw new InvalidOperationException($"未対応のチャンネル変換です: {sourceLayout} から {targetLayout}"),
        };
    }

    private static double MixSurroundToStereo(double[] samples, long frameOffset, int targetChannel)
    {
        double front = samples[frameOffset + targetChannel];
        double center = samples[frameOffset + 2];
        double surround = samples[frameOffset + (targetChannel == 0 ? 4 : 5)];
        return front + (SurroundMixCoefficient * (center + surround));
    }

    private static double MixSurroundToMono(double[] samples, long frameOffset)
    {
        double frontLeft = samples[frameOffset];
        double frontRight = samples[frameOffset + 1];
        double center = samples[frameOffset + 2];
        double backLeft = samples[frameOffset + 4];
        double backRight = samples[frameOffset + 5];
        return ((frontLeft + frontRight) * 0.5)
            + (SurroundMixCoefficient * (center + backLeft + backRight) * 0.5);
    }

    /// <summary>
    /// 対応していないチャンネル数同士の変換。インデックスをそのまま対応させ、モノラル化は全チャンネルの平均とする
    /// </summary>
    private static double GetChannelSampleFallback(IAudioChunk chunk, long frame, int targetChannel, int targetChannelCount)
    {
        var sourceFormat = chunk.Format;
        int sourceChannelCount = sourceFormat.ChannelCount;

        if (targetChannelCount == 1 && sourceChannelCount > 1)
        {
            double sum = 0;
            long frameOffset = frame * sourceChannelCount;
            for (int channel = 0; channel < sourceChannelCount; channel++)
            {
                sum += chunk.Samples[frameOffset + channel];
            }

            return sum / sourceChannelCount;
        }

        if (sourceChannelCount > targetChannel)
        {
            long index = (frame * sourceChannelCount) + targetChannel;
            return chunk.Samples[index];
        }

        if (sourceChannelCount == 1)
        {
            return chunk.Samples[frame];
        }

        long mappedIndex = (frame * sourceChannelCount) + (targetChannel % sourceChannelCount);
        return chunk.Samples[mappedIndex];
    }
}
