using Metasia.Core.Media;
using Metasia.Core.Objects;
using System.Collections.Generic;

namespace Metasia.Core.Sounds
{
    public class GetAudioContext
    {
        private static readonly IReadOnlyDictionary<string, TimelineObject> EmptyTimelineLookup
            = new Dictionary<string, TimelineObject>(StringComparer.OrdinalIgnoreCase);

        public IAudioFormat Format { get; }

        public long StartSamplePosition { get; } = 0;

        public long RequiredLength { get; } = 0;

        public double ProjectFrameRate { get; } = 60;

        /// <summary>
        /// オブジェクト全体の長さ（秒）
        /// </summary>
        public double ObjectDurationInSeconds { get; }

        public IAudioFileAccessor? AudioFileAccessor { get; }

        public string? ProjectPath { get; }

        public IReadOnlyDictionary<string, TimelineObject> AvailableTimelines { get; }

        public IReadOnlyList<string> TimelineReferenceStack { get; }

        public GetAudioContext(
            IAudioFormat format,
            long startSamplePosition,
            long requiredLength,
            double projectFrameRate,
            double objectDurationInSeconds,
            IAudioFileAccessor? audioFileAccessor = null,
            string? projectPath = null,
            IReadOnlyDictionary<string, TimelineObject>? availableTimelines = null,
            IReadOnlyList<string>? timelineReferenceStack = null)
        {
            ArgumentNullException.ThrowIfNull(format);
            if (startSamplePosition < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startSamplePosition), "startSamplePosition must be non-negative");
            }
            if (requiredLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredLength), "requiredLength must be non-negative");
            }
            if (double.IsNaN(projectFrameRate) || double.IsInfinity(projectFrameRate) || projectFrameRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectFrameRate), "projectFrameRate must be positive");
            }
            if (double.IsNaN(objectDurationInSeconds) || double.IsInfinity(objectDurationInSeconds) || objectDurationInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(objectDurationInSeconds), "objectDurationInSeconds must be non-negative");
            }
            Format = format;
            StartSamplePosition = startSamplePosition;
            RequiredLength = requiredLength;
            ProjectFrameRate = projectFrameRate;
            ObjectDurationInSeconds = objectDurationInSeconds;
            AudioFileAccessor = audioFileAccessor;
            ProjectPath = projectPath;
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

        public GetAudioContext CreateChildContext(
            long startSamplePosition,
            long requiredLength,
            double objectDurationInSeconds)
        {
            return new GetAudioContext(
                Format,
                startSamplePosition,
                requiredLength,
                ProjectFrameRate,
                objectDurationInSeconds,
                AudioFileAccessor,
                ProjectPath,
                AvailableTimelines,
                TimelineReferenceStack);
        }

        public GetAudioContext CreateReferencedTimelineContext(
            TimelineObject timeline,
            long startSamplePosition,
            long requiredLength,
            double objectDurationInSeconds)
        {
            ArgumentNullException.ThrowIfNull(timeline);

            return new GetAudioContext(
                Format,
                startSamplePosition,
                requiredLength,
                ProjectFrameRate,
                objectDurationInSeconds,
                AudioFileAccessor,
                ProjectPath,
                AvailableTimelines,
                AppendTimelineId(timeline.Id));
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
