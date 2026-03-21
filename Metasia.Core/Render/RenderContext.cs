using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render.Cache;
using SkiaSharp;
using System.Collections.Generic;

namespace Metasia.Core.Render
{
    public class RenderContext
    {
        private static readonly IReadOnlyDictionary<string, TimelineObject> EmptyTimelineLookup
            = new Dictionary<string, TimelineObject>(StringComparer.OrdinalIgnoreCase);

        public int Frame { get; init; }

        public SKSize ProjectResolution { get; init; }

        public SKSize RenderResolution { get; init; }

        public IImageFileAccessor ImageFileAccessor { get; init; }

        public IVideoFileAccessor VideoFileAccessor { get; init; }

        public ProjectInfo ProjectInfo { get; init; }

        public string ProjectPath { get; init; }

        public IRenderImageCache? ImageCache { get; init; }

        public IRenderSurfaceFactory SurfaceFactory { get; init; }

        public bool PreferRasterOutput { get; init; }

        public IReadOnlyDictionary<string, TimelineObject> AvailableTimelines { get; init; }

        public IReadOnlyList<string> TimelineReferenceStack { get; init; }

        public RenderContext(
            int frame,
            SKSize projectResolution,
            SKSize renderResolution,
            IImageFileAccessor imageFileAccessor,
            IVideoFileAccessor videoFileAccessor,
            ProjectInfo projectInfo,
            string projectPath,
            IRenderImageCache? imageCache = null,
            IRenderSurfaceFactory? surfaceFactory = null,
            bool preferRasterOutput = false,
            IReadOnlyDictionary<string, TimelineObject>? availableTimelines = null,
            IReadOnlyList<string>? timelineReferenceStack = null)
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
            ProjectPath = projectPath ?? string.Empty;
            ImageCache = imageCache;
            SurfaceFactory = surfaceFactory ?? new NullRenderSurfaceFactory();
            PreferRasterOutput = preferRasterOutput;
            AvailableTimelines = availableTimelines ?? EmptyTimelineLookup;
            TimelineReferenceStack = timelineReferenceStack ?? Array.Empty<string>();
        }

        public bool TryResolveTimeline(string timelineId, out TimelineObject? timeline)
        {
            timeline = null;
            if (string.IsNullOrWhiteSpace(timelineId))
            {
                return false;
            }

            return AvailableTimelines.TryGetValue(timelineId, out timeline);
        }

        public bool IsTimelineInReferenceStack(string timelineId)
        {
            if (string.IsNullOrWhiteSpace(timelineId))
            {
                return false;
            }

            foreach (var item in TimelineReferenceStack)
            {
                if (string.Equals(item, timelineId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public RenderContext CreateReferencedTimelineContext(TimelineObject timeline, int frame)
        {
            ArgumentNullException.ThrowIfNull(timeline);

            return new RenderContext(
                frame,
                ProjectResolution,
                RenderResolution,
                ImageFileAccessor,
                VideoFileAccessor,
                ProjectInfo,
                ProjectPath,
                imageCache: ImageCache,
                surfaceFactory: SurfaceFactory,
                preferRasterOutput: PreferRasterOutput,
                availableTimelines: AvailableTimelines,
                timelineReferenceStack: AppendTimelineId(timeline.Id));
        }

        private IReadOnlyList<string> AppendTimelineId(string timelineId)
        {
            if (string.IsNullOrWhiteSpace(timelineId))
            {
                return TimelineReferenceStack;
            }

            string[] result = new string[TimelineReferenceStack.Count + 1];
            for (int i = 0; i < TimelineReferenceStack.Count; i++)
            {
                result[i] = TimelineReferenceStack[i];
            }

            result[^1] = timelineId;
            return result;
        }
    }
}
