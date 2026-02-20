using Metasia.Core.Objects;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector;

public interface IAudioEffectsViewModelFactory
{
    AudioEffectsViewModel Create(IAudible target);
}

