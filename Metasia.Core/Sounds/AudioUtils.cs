


using System;

namespace Metasia.Core.Sounds
{
    /// <summary>
    /// Utility class for audio operations
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Mixes multiple audio frames together
        /// </summary>
        public static AudioFrame Mix(params AudioFrame[] frames)
        {
            return AudioFrame.Mix(frames);
        }

        /// <summary>
        /// Adjusts the volume of an audio frame
        /// </summary>
        public static AudioFrame ChangeVolume(AudioFrame frame, double volume)
        {
            return AudioFrame.ChangeVolume(frame, volume);
        }
    }
}

