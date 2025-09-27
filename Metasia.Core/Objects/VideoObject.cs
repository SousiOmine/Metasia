using System.Diagnostics;
using System.Net;
using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("VideoObject")]
public class VideoObject : ClipObject, IRenderable
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

	public VideoObject()
	{

	}

	public VideoObject(string id) : base(id)
	{

	}

	public RenderNode Render(RenderContext context)
	{
		int relativeFrame = context.Frame - StartFrame;
		if (VideoPath is not null && !string.IsNullOrEmpty(VideoPath?.FileName))
		{
			try
			{
				TimeSpan time = TimeSpan.FromSeconds((relativeFrame) / context.ProjectInfo.Framerate + VideoStartSeconds.Get(relativeFrame));
				var imageFileAccessorResult = context.VideoFileAccessor.GetBitmap(VideoPath, time, "");
				if (imageFileAccessorResult.IsSuccessful && imageFileAccessorResult.Bitmap is not null)
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
				Debug.WriteLine($"Failed to load video: {ex.Message}");
			}
		}
		Debug.WriteLine($"Failed to load video: {VideoPath}");
		return new RenderNode();
	}
}