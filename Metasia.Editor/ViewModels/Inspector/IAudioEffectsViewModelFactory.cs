using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels.Inspector;

public interface IAudioEffectsViewModelFactory
{
    AudioEffectsViewModel Create(IAudible target);
}

