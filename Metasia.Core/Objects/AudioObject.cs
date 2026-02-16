using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Media;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("AudioObject")]
public class AudioObject : ClipObject, IAudible
{
    [EditableProperty("AudioPath")]
    public MediaPath AudioPath { get; set; } = new();

    [EditableProperty("AudioStartSeconds")]
    [ValueRange(0, 99999, 0, 3600)]
    public MetaNumberParam<double> AudioStartSeconds { get; set; } = new(0);

    [EditableProperty("AudioVolume")]
    [ValueRange(0, 99999, 0, 200)]
    public MetaDoubleParam Volume { get; set; } = new(100);

    public List<AudioEffectBase> AudioEffects { get; } = new();

    public AudioObject()
    {
        AudioPath = new MediaPath([MediaType.Audio]);
    }

    public AudioObject(string id) : base(id)
    {
        AudioPath = new MediaPath([MediaType.Audio]);
    }

    public async Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
    {
        IAudioChunk result = new AudioChunk(context.Format, context.RequiredLength);

        if (context.RequiredLength <= 0)
        {
            return ApplyEffects(result, context);
        }

        if (AudioPath is null || string.IsNullOrWhiteSpace(AudioPath.FileName) || context.AudioFileAccessor is null)
        {
            return ApplyEffects(result, context);
        }

        try
        {
            string fullPath = MediaPath.GetFullPath(AudioPath, context.ProjectPath);
            double startSeconds = AudioStartSeconds.Get(0, 1) + (context.StartSamplePosition / (double)context.Format.SampleRate);
            if (startSeconds < 0)
            {
                startSeconds = 0;
            }

            double durationSeconds = context.RequiredLength / (double)context.Format.SampleRate;
            var accessorResult = await context.AudioFileAccessor
                .GetAudioAsync(fullPath, TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(durationSeconds));

            if (!accessorResult.IsSuccessful || accessorResult.Chunk is null)
            {
                return ApplyEffects(result, context);
            }

            result = AudioChunkConverter.ConvertToFormat(accessorResult.Chunk, context.Format, context.RequiredLength);
            double gain = Volume.Value / 100.0;
            for (long i = 0; i < result.Samples.Length; i++)
            {
                result.Samples[i] = Math.Clamp(result.Samples[i] * gain, -1.0, 1.0);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load audio: {AudioPath}. {ex.Message}");
            return ApplyEffects(result, context);
        }

        return ApplyEffects(result, context);
    }

    private IAudioChunk ApplyEffects(IAudioChunk chunk, GetAudioContext context)
    {
        IAudioChunk result = chunk;
        AudioEffectContext effectContext = new(this, context);

        foreach (var effect in AudioEffects)
        {
            result = effect.Apply(result, effectContext);
        }

        return result;
    }
}
