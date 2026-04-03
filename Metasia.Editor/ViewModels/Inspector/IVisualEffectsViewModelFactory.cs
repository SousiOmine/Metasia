using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Inspector;

public interface IVisualEffectsViewModelFactory
{
    VisualEffectsViewModel Create(IRenderable target);
}
