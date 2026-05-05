using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects.Clips;

[Serializable]
[ClipTypeIdentifier("CameraControlObject", DisplayKey = "clip.camera_control.name", FallbackText = "カメラ制御")]
public class CameraControlObject : ClipObject, IRenderable, ILayerIntervener, IDisposable
{
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

    [EditableProperty("TargetLayers", DisplayKey = "property.common.target_layers", FallbackText = "対象レイヤー")]
    public LayerTarget TargetLayers { get; set; } = new LayerTarget(5);

    public List<VisualEffectBase> VisualEffects { get; set; } = new();

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
            ScopeLayerTarget = TargetLayers,
            VisualEffects = VisualEffects,
            VisualEffectContext = VisualEffectContext.FromRenderContext(context, StartFrame, EndFrame, context.ProjectResolution)
        });
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
