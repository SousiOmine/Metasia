using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("AudioObject", DisplayKey = "clip.audio.name", FallbackText = "音声")]
public class AudioObject : ClipObject, IAudible
{
    [EditableProperty("AudioPath", DisplayKey = "property.audio.path", FallbackText = "音声ファイル")]
    public MediaPath AudioPath { get; set; } = new();

    [EditableProperty("AudioStartSeconds", DisplayKey = "property.common.audio_start_seconds", FallbackText = "音声開始秒")]
    [ValueRange(0, 99999, 0, 3600)]
    public MetaNumberParam<double> AudioStartSeconds { get; set; } = new(0);

    [EditableProperty("AudioVolume", DisplayKey = "property.common.audio_volume", FallbackText = "音量")]
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

    public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame, SplitContext? context = null)
    {
        var result = base.SplitAtFrame(splitFrame, context);

        if (context is null) return result;

        var first = (AudioObject)result.firstClip;
        var second = (AudioObject)result.secondClip;
        int relativeSplitFrame = splitFrame - StartFrame;
        int oldClipLength = EndFrame - StartFrame + 1;
        var (firstAudioStartSeconds, secondAudioStartSeconds) = AudioStartSeconds.Split(relativeSplitFrame, oldClipLength);
        double timeOffset = (splitFrame - StartFrame) / context.FrameRate;
        first.AudioStartSeconds = firstAudioStartSeconds;
        second.AudioStartSeconds = ShiftMetaNumberParamSeconds(secondAudioStartSeconds, timeOffset);

        return result;
    }

    private static MetaNumberParam<double> ShiftMetaNumberParamSeconds(MetaNumberParam<double> param, double offsetSeconds)
    {
        var shifted = new MetaNumberParam<double>();
        shifted.IsMovable = param.IsMovable;

        if (!param.IsMovable)
        {
            shifted.SetSinglePoint(param.StartPoint.Value + offsetSeconds);
            return shifted;
        }

        shifted.StartPoint = new CoordPoint
        {
            Frame = param.StartPoint.Frame,
            Value = param.StartPoint.Value + offsetSeconds,
            InterpolationLogic = param.StartPoint.InterpolationLogic.HardCopy()
        };
        shifted.EndPoint = new CoordPoint
        {
            Frame = param.EndPoint.Frame,
            Value = param.EndPoint.Value + offsetSeconds,
            InterpolationLogic = param.EndPoint.InterpolationLogic.HardCopy()
        };

        foreach (var pt in param.Params)
        {
            shifted.AddPoint(new CoordPoint
            {
                Frame = pt.Frame,
                Value = pt.Value + offsetSeconds,
                InterpolationLogic = pt.InterpolationLogic.HardCopy()
            });
        }

        return shifted;
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
            long audioStartSample = (long)(AudioStartSeconds.Get(0, 1) * context.Format.SampleRate);
            long mediaStartSample = audioStartSample + context.StartSamplePosition;
            if (mediaStartSample < 0)
            {
                mediaStartSample = 0;
            }

            var accessorResult = await context.AudioFileAccessor
                .GetAudioBySampleAsync(fullPath, mediaStartSample, context.RequiredLength, context.Format.SampleRate);

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
            if (!effect.IsActive) continue;
            result = effect.Apply(result, effectContext);
        }

        return result;
    }
}
