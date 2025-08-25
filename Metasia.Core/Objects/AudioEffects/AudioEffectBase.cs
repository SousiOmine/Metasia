using Metasia.Core.Sounds;

namespace Metasia.Core.Objects.AudioEffects
{
	public abstract class AudioEffectBase : IAudioEffect
	{
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public abstract IAudioChunk Apply(IAudioChunk input, AudioEffectContext context);
	}
}
