using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Objects.Clips;

[ClipTypeIdentifier("VideoObject", DisplayKey = "clip.video.name", FallbackText = "動画")]
public class VideoObject : ClipObject, IRenderable, IAudible
{
    [EditableProperty("BlendMode", DisplayKey = "property.common.blend_mode", FallbackText = "合成モード")]
    public BlendModeParam BlendMode { get; set; } = new BlendModeParam();

    [EditableProperty("X", DisplayKey = "property.common.x", FallbackText = "X")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Y", DisplayKey = "property.common.y", FallbackText = "Y")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Scale", DisplayKey = "property.common.scale", FallbackText = "拡大率")]
    [ValueRange(0, 99999, 0, 1000)]
    public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);
    [EditableProperty("Alpha", DisplayKey = "property.common.alpha", FallbackText = "透明度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Rotation", DisplayKey = "property.common.rotation", FallbackText = "回転")]
    [ValueRange(-99999, 99999, 0, 360)]
    public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("VideoPath", DisplayKey = "property.video.path", FallbackText = "動画ファイル")]
    public MediaPath VideoPath { get; set; } = new MediaPath();

    [EditableProperty("VideoStartSeconds", DisplayKey = "property.video.start_seconds", FallbackText = "再生開始位置(s)")]
    [ValueRange(0, 99999, 0, 3600)]
    public MetaNumberParam<double> VideoStartSeconds { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("Speed", DisplayKey = "property.common.speed", FallbackText = "再生速度")]
    [ValueRange(1, 99999, 10, 1000)]
    public MetaDoubleParam Speed { get; set; } = new MetaDoubleParam(100);

    [EditableProperty("AudioVolume", DisplayKey = "property.common.audio_volume", FallbackText = "音量")]
    [ValueRange(0, 99999, 0, 200)]
    public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

    public List<AudioEffectBase> AudioEffects { get; set; } = new();

    public List<VisualEffectBase> VisualEffects { get; set; } = new();

    public VideoObject()
    {
        VideoPath = new MediaPath([MediaType.Video]);
    }

    public VideoObject(string id) : base(id)
    {
        VideoPath = new MediaPath([MediaType.Video]);
    }

    public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame, SplitContext? context = null)
    {
        var result = base.SplitAtFrame(splitFrame, context);

        if (context is null) return result;

        var first = (VideoObject)result.firstClip;
        var second = (VideoObject)result.secondClip;
        int relativeSplitFrame = splitFrame - StartFrame;
        int oldClipLength = EndFrame - StartFrame + 1;
        var (firstVideoStartSeconds, secondVideoStartSeconds) = VideoStartSeconds.Split(relativeSplitFrame, oldClipLength);
        double timeOffset = ((splitFrame - StartFrame) / context.FrameRate) * (Speed.Value / 100.0);
        first.VideoStartSeconds = firstVideoStartSeconds;
        second.VideoStartSeconds = ShiftMetaNumberParamSeconds(secondVideoStartSeconds, timeOffset);

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

    public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
        int relativeFrame = context.Frame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;
        if (VideoPath is not null && !string.IsNullOrEmpty(VideoPath?.FileName))
        {
            try
            {
                double speed = Speed.Value / 100.0;
                TimeSpan time = TimeSpan.FromSeconds((double)(relativeFrame * speed) / context.ProjectInfo.Framerate + VideoStartSeconds.Get(relativeFrame, clipLength));
                var imageFileAccessorResult = await context.VideoFileAccessor.GetImageAsync(MediaPath.GetFullPath(VideoPath, context.ProjectPath), time);
                if (imageFileAccessorResult.IsSuccessful && imageFileAccessorResult.Image is not null)
                {
                    var transform = new Transform()
                    {
                        Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                        Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                        Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                        Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
                    };
                    var logicalSize = new SKSize(imageFileAccessorResult.Image.Width, imageFileAccessorResult.Image.Height);
                    var finalResult = VisualEffectPipeline.ApplyEffects(imageFileAccessorResult.Image, VisualEffects, context, StartFrame, EndFrame, logicalSize);
                    var finalTransform = finalResult.TransformOffset is not null
                        ? transform.Add(finalResult.TransformOffset)
                        : transform;
                    return new NormalRenderNode()
                    {
                        Image = finalResult.Image,
                        LogicalSize = finalResult.LogicalSize,
                        Transform = finalTransform,
                        BlendMode = BlendMode.Value,
                        ImageCacheKey = finalResult.ImageCacheKey,
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load video: {VideoPath}. {ex.Message}");
                return new NormalRenderNode();
            }
        }
        Debug.WriteLine($"Failed to load video: {VideoPath}");
        return new NormalRenderNode();
    }

    public async Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
    {
        IAudioChunk result = new AudioChunk(context.Format, context.RequiredLength);

        if (context.RequiredLength <= 0)
        {
            return result;
        }

        if (VideoPath is null || string.IsNullOrWhiteSpace(VideoPath.FileName) || context.AudioFileAccessor is null)
        {
            return ApplyEffects(result, context);
        }

        try
        {
            string fullPath = MediaPath.GetFullPath(VideoPath, context.ProjectPath);
            int clipLength = EndFrame - StartFrame + 1;
            int relativeFrame = (int)((context.StartSamplePosition / (double)context.Format.SampleRate) * context.ProjectFrameRate);
            double videoStartSecondsValue = VideoStartSeconds.Get(relativeFrame, clipLength);
            long videoStartSample = (long)(videoStartSecondsValue * context.Format.SampleRate);
            double speed = Speed.Value / 100.0;
            long mediaStartSample = videoStartSample + (long)(context.StartSamplePosition * speed);
            if (mediaStartSample < 0)
            {
                mediaStartSample = 0;
            }

            long sourceLength = (long)(context.RequiredLength * speed);
            var accessorResult = await context.AudioFileAccessor
                .GetAudioBySampleAsync(fullPath, mediaStartSample, sourceLength, context.Format.SampleRate);

            if (!accessorResult.IsSuccessful || accessorResult.Chunk is null)
            {
                return ApplyEffects(result, context);
            }

            if (Math.Abs(speed - 1.0) > 0.001)
            {
                var sourceFormat = new AudioFormat((int)(context.Format.SampleRate * speed), context.Format.ChannelCount);
                var sourceChunk = new AudioChunk(sourceFormat, accessorResult.Chunk.Samples);
                result = AudioChunkConverter.ConvertToFormat(sourceChunk, context.Format, context.RequiredLength);
            }
            else
            {
                result = AudioChunkConverter.ConvertToFormat(accessorResult.Chunk, context.Format, context.RequiredLength);
            }

            double gain = Volume.Value / 100.0;
            for (long i = 0; i < result.Samples.Length; i++)
            {
                result.Samples[i] = Math.Clamp(result.Samples[i] * gain, -1.0, 1.0);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load video audio: {VideoPath}. {ex.Message}");
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
