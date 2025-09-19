using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Render;

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
    public MediaPath? ImagePath { get; set; } = null;

    public ImageObject()
	{

	}

	public ImageObject(string id) : base(id)
	{

	}

    public RenderNode Render(RenderContext context)
    {
        throw new NotImplementedException();
    }
}