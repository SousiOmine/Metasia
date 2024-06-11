using Metasia.Core.Coordinate;

namespace Metasia.Core.Objects;

public interface IMetaCoordable : IMetaDrawable
{
    public List<CoordPoint> X_Points { get; protected set; }
    public List<CoordPoint> Y_Points { get; protected set; }
    public List<CoordPoint> Scale_Points { get; protected set; }
    public List<CoordPoint> Alpha_Points { get; protected set; }
    public List<CoordPoint> Rotation_Points { get; protected set; }
    
    public float X { get; protected set; }
    public float Y { get; protected set; }
    public float Scale { get; protected set; }
    public float Alpha { get; protected set; }
    public float Rotation { get; protected set; }
}