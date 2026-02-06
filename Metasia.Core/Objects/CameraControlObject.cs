using Metasia.Core.Attributes;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Objects;

[Serializable]
[ClipTypeIdentifier("CameraControlObject")]
public class CameraControlObject : ClipObject, IRenderable, IAudible, ILayerIntervener, IDisposable
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

    public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

    [EditableProperty("TargetLayerCount")]
    [ValueRange(-1, 99999, -1, 99999)]
    public int TargetLayerCount 
    {
        get => _targetLayerCount;
        set  {
            if (value < 0)
            {
                _targetLayerCount = -1;
            }
            else
            {
                _targetLayerCount = value;
            }
        }
    }

    public List<AudioEffectBase> AudioEffects { get; set; } = new();

    

    private int _targetLayerCount = 5;
    private bool disposed;

    public CameraControlObject()
    {
        
    }

    public CameraControlObject(string id) : base(id)
    {
        
    }


    public Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
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
        return Task.FromResult<IRenderNode>(new CameraControlRenderNode()
        {
            Transform = transform,
            ScopeLayerCount = TargetLayerCount,
        });
    }

    public IAudioChunk GetAudioChunk(GetAudioContext context)
    {
        throw new NotImplementedException();
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