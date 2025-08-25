namespace Metasia.Core.Sounds
{
    public class GetAudioContext
    {
        public AudioFormat Format { get; }

        public long StartSamplePosition { get; } = 0;

        public long RequiredLength { get; } = 0;

        public double ProjectFrameRate { get; } = 60;

        /// <summary>
        /// オブジェクト全体の長さ（秒）
        /// </summary>
        public double ObjectDurationInSeconds { get; }

        public GetAudioContext(AudioFormat format, long startSamplePosition, long requiredLength, double projectFrameRate, double objectDurationInSeconds)
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
        }
    }
}
