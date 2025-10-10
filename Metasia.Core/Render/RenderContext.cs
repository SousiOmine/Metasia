using Metasia.Core.Media;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Core.Render
{
    public class RenderContext
    {
        public int Frame { get; init; }

        public SKSize ProjectResolution { get; init; }

        public SKSize RenderResolution { get; init; }

        public IImageFileAccessor ImageFileAccessor { get; init; }

        public IVideoFileAccessor VideoFileAccessor { get; init; }

        public ProjectInfo ProjectInfo { get; init; }

        public RenderContext(int frame, SKSize projectResolution, SKSize renderResolution, IImageFileAccessor imageFileAccessor, IVideoFileAccessor videoFileAccessor, ProjectInfo projectInfo)
        {
            Frame = frame;

            ArgumentNullException.ThrowIfNull(projectInfo);

            if (projectResolution.Width <= 0 || projectResolution.Height <= 0
                || float.IsInfinity(projectResolution.Width) || float.IsInfinity(projectResolution.Height)
                || float.IsNaN(projectResolution.Width) || float.IsNaN(projectResolution.Height))
                throw new ArgumentOutOfRangeException(nameof(projectResolution), "Project resolution must be positive");
            if (renderResolution.Width <= 0 || renderResolution.Height <= 0
                || float.IsInfinity(renderResolution.Width) || float.IsInfinity(renderResolution.Height)
                || float.IsNaN(renderResolution.Width) || float.IsNaN(renderResolution.Height))
                throw new ArgumentOutOfRangeException(nameof(renderResolution), "Render resolution must be positive");

            ProjectResolution = projectResolution;
            RenderResolution = renderResolution;

            ImageFileAccessor = imageFileAccessor;
            VideoFileAccessor = videoFileAccessor;
            ProjectInfo = projectInfo;
        }
    }
}