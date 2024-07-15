using Metasia.Core.Coordinate;

namespace Metasia.Core.Objects;

public interface IMetaCoordable : IMetaDrawable
{
    public MetaDoubleParam X { get; set; }
    public MetaDoubleParam Y { get; set; }
    public MetaDoubleParam Scale { get; set; }
    public MetaDoubleParam Alpha { get; set; }
    public MetaDoubleParam Rotation { get; set; }
    
}