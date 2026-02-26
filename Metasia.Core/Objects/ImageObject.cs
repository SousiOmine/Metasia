using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("ImageObject")]
public class ImageObject : ClipObject, IRenderable
{
    [EditableProperty("BlendMode")]
    public BlendModeParam BlendMode { get; set; } = new BlendModeParam();

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

    [EditableProperty("ImagePath")]
    public MediaPath ImagePath { get; set; } = new MediaPath();

    public List<VisualEffectBase> VisualEffects { get; set; } = new();

    public ImageObject()
    {
        ImagePath = new MediaPath([Media.MediaType.Image]);
    }

    public ImageObject(string id) : base(id)
    {
        ImagePath = new MediaPath([Media.MediaType.Image]);
    }

    public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int relativeFrame = context.Frame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;
        if (ImagePath is null || string.IsNullOrWhiteSpace(ImagePath.FileName))
        {
            return new NormalRenderNode();
        }

        try
        {
            var imageFileAccessorResult = await context.ImageFileAccessor.GetImageAsync(MediaPath.GetFullPath(ImagePath, context.ProjectPath));
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
                var finalImage = VisualEffectPipeline.ApplyEffects(imageFileAccessorResult.Image, VisualEffects, context, StartFrame, EndFrame, logicalSize);
                return new NormalRenderNode()
                {
                    Image = finalImage,
                    LogicalSize = logicalSize,
                    Transform = transform,
                    BlendMode = BlendMode.Value,
                };
            }

            Debug.WriteLine($"Failed to load image: {ImagePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load image: {ImagePath}. {ex.Message}");
        }
        return new NormalRenderNode();
    }
}
