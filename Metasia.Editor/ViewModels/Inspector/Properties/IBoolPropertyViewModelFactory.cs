using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IBoolPropertyViewModelFactory
{
    BoolPropertyViewModel Create(string propertyIdentifier, bool target, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}