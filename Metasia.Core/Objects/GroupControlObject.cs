using Metasia.Core.Attributes;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Objects;

[Serializable]
[ClipTypeIdentifier("GroupControlObject")]
public class GroupControlObject : ClipObject, IRenderable, IAudible, ILayerIntervener, IDisposable
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

    [EditableProperty("AudioVolume")]
    [ValueRange(0, 99999, 0, 200)]
    public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

    [EditableProperty("TargetLayers")]
    public LayerTarget TargetLayers { get; set; } = new LayerTarget(5);

    public List<AudioEffectBase> AudioEffects { get; set; } = new();

    private bool disposed;

    public GroupControlObject()
    {

    }

    public GroupControlObject(string id) : base(id)
    {

    }

    ~GroupControlObject()
    {
        Dispose(false);
    }
    public Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        //このオブジェクトのStartFrameを基準としたフレーム
        int relativeFrame = context.Frame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;

        var transform = new Transform()
        {
            Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
            Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
            Rotation = (float)Rotation.Get(relativeFrame, clipLength),
            Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
        };
        return Task.FromResult<IRenderNode>(new GroupControlRenderNode()
        {
            Transform = transform,
            ScopeLayerTarget = TargetLayers,
        });
    }

    public Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
    {
        // GroupControlObject自体は音声を生成しないが、IAudibleインターフェースを実装するため空のチャンクを返す
        IAudioChunk chunk = new AudioChunk(context.Format, context.RequiredLength);

        // 音量とエフェクトはTimelineObjectで対象レイヤーの音声に適用される
        return Task.FromResult(chunk);
    }



    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }
}
