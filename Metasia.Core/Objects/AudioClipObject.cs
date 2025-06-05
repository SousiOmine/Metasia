


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
        private IAudioSource _audioSource;
        private int _audioOffset;

        /// <summary>
        /// Volume level (0.0 to 1.0)
        /// </summary>
        public double Volume { get; set; } = 1.0;

        /// <summary>
        /// Creates a new audio clip object with a sine wave source
        /// </summary>
        [JsonConstructor]
        public AudioClipObject()
        {
            _audioSource = new SineWaveSource();
        }

        /// <summary>
        /// Creates a new audio clip object with the specified audio source
        /// </summary>
        /// <param name="audioSource">The audio source to use</param>
        public AudioClipObject(IAudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        /// <summary>
        /// Expresses the audio for the specified frame
        /// </summary>
        /// <param name="e">Audio expression arguments</param>
        /// <param name="frame">The frame number to generate audio for</param>
        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            // Generate audio frame from the audio source
            AudioFrame audioFrame = _audioSource.GenerateAudioFrame(
                e.AudioChannel,
                e.SoundSampleRate,
                (ushort)e.FPS,
                frame
            );

            // Adjust volume
            if (Volume != 1.0)
            {
                audioFrame = AudioUtils.ChangeVolume(audioFrame, (double)Volume);
            }

            // Convert to MetasiaSound for compatibility
            e.Sound = new MetasiaSound(
                audioFrame.Pulse.ToArray(),
                audioFrame.Channel,
                audioFrame.SampleRate,
                audioFrame.FPS
            );
        }
    }
}

