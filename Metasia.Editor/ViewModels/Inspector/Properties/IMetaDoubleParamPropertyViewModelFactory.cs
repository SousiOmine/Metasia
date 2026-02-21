using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaDoubleParamPropertyViewModelFactory
{
    MetaDoubleParamPropertyViewModel Create(string propertyIdentifier, MetaDoubleParam target, double min = double.MinValue, double max = double.MaxValue, double recommendMin = double.MinValue, double recommendMax = double.MaxValue);
}