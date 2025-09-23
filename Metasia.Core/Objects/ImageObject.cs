using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

public class ImageObject : ClipObject, IRenderable
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

    [EditableProperty("ImagePath")]
    public MediaPath ImagePath { get; set; } = new MediaPath();

    public ImageObject()
	{

	}

	public ImageObject(string id) : base(id)
	{

	}

    public RenderNode Render(RenderContext context)
    {
		int relativeFrame = context.Frame - StartFrame;
        if(ImagePath is not null || !string.IsNullOrEmpty(ImagePath?.FileName))
		{
			try
			{
				var imageFileAccessorResult = context.ImageFileAccessor.GetBitmap(ImagePath);
				if(imageFileAccessorResult.IsSucceed && imageFileAccessorResult.Bitmap is not null)
				{
					var transform = new Transform()
					{
						Position = new SKPoint((float)X.Get(relativeFrame), (float)Y.Get(relativeFrame)),
						Scale = (float)Scale.Get(relativeFrame) / 100,
						Rotation = (float)Rotation.Get(relativeFrame),
						Alpha = (100.0f - (float)Alpha.Get(relativeFrame)) / 100,
					};
					return new RenderNode()
					{
						Bitmap = imageFileAccessorResult.Bitmap,
						LogicalSize = new SKSize(imageFileAccessorResult.Bitmap.Width, imageFileAccessorResult.Bitmap.Height),
						Transform = transform,
					};
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load image: {ex.Message}");
			}
		}
		Debug.WriteLine($"Failed to load image: {ImagePath}");
		return new RenderNode();
    }
}