using SkiaSharp;

namespace Metasia.Core.Render
{
    public class RenderContext
    {
        public int Frame { get; init; }

        public SKSize ProjectResolution { get; init; }

        public SKSize RenderResolution { get; init; }

        public RenderContext(int frame, SKSize projectResolution, SKSize renderResolution)
        {
            Frame = frame;
            ProjectResolution = projectResolution;
            RenderResolution = renderResolution;
        }
    }
}