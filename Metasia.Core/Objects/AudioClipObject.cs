


using Metasia.Core.Render;
using Metasia.Core.Sounds;
using System;
using System.Text.Json.Serialization;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// Represents an audio clip object that can generate audio
    /// </summary>
    public class AudioClipObject : MetasiaObject, IMetaAudiable
    {
        /// <summary>
        /// The audio source for this clip
        /// </summary>
        public IAudioSource AudioSource { get; set; }

        /// <summary>
        /// Volume level (0.0 to 1.0)
        /// </summary>
        public double Volume { get; set; } = 1.0;

        /// <summary>
        /// Creates a new audio clip object
        /// </summary>
        [JsonConstructor]
        public AudioClipObject()
        {
            // Default to silence
            AudioSource = new SilenceSource();
        }

        /// <summary>
        /// Creates a new audio clip object with a specific audio source
        /// </summary>
        public AudioClipObject(string id, IAudioSource audioSource) : base(id)
        {
            AudioSource = audioSource ?? new SilenceSource();
        }

        /// <summary>
        /// Generates audio for a specific frame
        /// </summary>
        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            if (frame < StartFrame || frame > EndFrame)
            {
                // Return silence if outside the object's time range
                e.Sound = null;
                return;
            }

            // Get the audio frame from the audio source
            AudioFrame audioFrame = AudioSource.GetAudioFrame(
                e.AudioChannel,
                e.SoundSampleRate,
                (ushort)e.FPS,
                frame - StartFrame
            );

            // Adjust the volume if needed
            if (Volume != 1.0)
            {
                audioFrame = AudioUtils.ChangeVolume(audioFrame, Volume);
            }

            // Convert to MetasiaSound for backward compatibility
            e.Sound = ConvertToMetasiaSound(audioFrame);
        }

        /// <summary>
        /// Converts an AudioFrame to a MetasiaSound for backward compatibility
        /// </summary>
        private MetasiaSound ConvertToMetasiaSound(AudioFrame audioFrame)
        {
            // Create a new MetasiaSound with the same parameters
            MetasiaSound metasiaSound = new MetasiaSound(
                audioFrame.ChannelCount,
                audioFrame.SampleRate,
                audioFrame.FPS
            );

            // Copy the samples
            audioFrame.Samples.Span.CopyTo(metasiaSound.Pulse);

            return metasiaSound;
        }
    }
}

