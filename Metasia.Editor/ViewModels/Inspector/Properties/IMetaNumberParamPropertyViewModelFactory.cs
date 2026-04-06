using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.ViewModels.Inspector.Properties.Components;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaNumberParamPropertyViewModelFactory
{
    MetaNumberParamPropertyViewModel Create(string propertyIdentifier, MetaNumberParam<double> target, double min = double.MinValue, double max = double.MaxValue, double recommendedMin = double.MinValue, double recommendedMax = double.MaxValue);
}
