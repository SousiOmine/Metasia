namespace Metasia.Core.Objects.Parameters.Color;

public class ColorRgb8
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public ColorRgb8()
        : this(255, 255, 255)
    {
    }

    public ColorRgb8(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public ColorRgb8 Clone()
    {
        return new ColorRgb8(R, G, B);
    }
}
