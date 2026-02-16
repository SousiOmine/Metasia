using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("VideoObject")]
public class VideoObject : ClipObject, IRenderable, IAudible
{
    [EditableProperty("X")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Y")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Scale")]
    [ValueRange(0, 99999, 0, 1000)]
    public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);
    [EditableProperty("Alpha")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);
    [EditableProperty("Rotation")]
    [ValueRange(-99999, 99999, 0, 360)]
    public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("VideoPath")]
    public MediaPath VideoPath { get; set; } = new MediaPath();

    [EditableProperty("VideoStartSeconds")]
    [ValueRange(0, 99999, 0, 3600)]
    public MetaNumberParam<double> VideoStartSeconds { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("AudioVolume")]
    [ValueRange(0, 99999, 0, 200)]
    public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

    public List<AudioEffectBase> AudioEffects { get; set; } = new();

    public VideoObject()
    {
        VideoPath = new MediaPath([MediaType.Video]);
    }

    public VideoObject(string id) : base(id)
    {
        VideoPath = new MediaPath([MediaType.Video]);
    }

    public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
        int relativeFrame = context.Frame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;
        if (VideoPath is not null && !string.IsNullOrEmpty(VideoPath?.FileName))
        {
            try
            {
                TimeSpan time = TimeSpan.FromSeconds((double)(relativeFrame) / context.ProjectInfo.Framerate + VideoStartSeconds.Get(relativeFrame, clipLength));
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
                    return new NormalRenderNode()
                    {
                        Image = imageFileAccessorResult.Image,
                        LogicalSize = new SKSize(imageFileAccessorResult.Image.Width, imageFileAccessorResult.Image.Height),
                        Transform = transform,
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load video: {ex.Message}");
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
            double startSeconds = VideoStartSeconds.Get(relativeFrame, clipLength) + (context.StartSamplePosition / (double)context.Format.SampleRate);
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
            result = effect.Apply(result, effectContext);
        }

        return result;
    }
}
