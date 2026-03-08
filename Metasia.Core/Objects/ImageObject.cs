using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("ImageObject", DisplayKey = "clip.image.name", FallbackText = "画像")]
public class ImageObject : ClipObject, IRenderable
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

    [EditableProperty("ImagePath", DisplayKey = "property.image.path", FallbackText = "画像ファイル")]
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

        long imageHashCode = GetImageHashCode();
        SKImage? image = context?.ImageCache?.TryGet(imageHashCode);

        if (image is null)
        {
            try
            {
                var imageFileAccessorResult = await context.ImageFileAccessor.GetImageAsync(MediaPath.GetFullPath(ImagePath, context.ProjectPath));
                if (imageFileAccessorResult.IsSuccessful && imageFileAccessorResult.Image is not null)
                {
                    image = imageFileAccessorResult.Image;
                    context?.ImageCache?.Set(imageHashCode, imageFileAccessorResult.Image);
                }

                Debug.WriteLine($"Failed to load image: {ImagePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load image: {ImagePath}. {ex.Message}");
            }
        }

        if (image is null) return new NormalRenderNode();

        var transform = new Transform()
        {
            Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
            Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
            Rotation = (float)Rotation.Get(relativeFrame, clipLength),
            Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
        };
        var logicalSize = new SKSize(image.Width, image.Height);
        var finalResult = VisualEffectPipeline.ApplyEffects(image, VisualEffects, context, StartFrame, EndFrame, logicalSize, imageCacheKey: imageHashCode);
        return new NormalRenderNode()
        {
            Image = finalResult.Image,
            LogicalSize = logicalSize,
            Transform = transform,
            BlendMode = BlendMode.Value,
            ImageCacheKey = finalResult.ImageCacheKey,
        };
    }

    private long GetImageHashCode()
    {
        var hash = new HashCode();
        hash.Add(nameof(ImageObject));
        hash.Add(ImagePath.FileName);

        return hash.ToHashCode();
    }
}
