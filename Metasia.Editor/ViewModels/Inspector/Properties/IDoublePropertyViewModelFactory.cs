using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IDoublePropertyViewModelFactory
{
    DoublePropertyViewModel Create(string propertyIdentifier, double target, double min = double.MinValue, double max = double.MaxValue, double recommendMin = double.MinValue, double recommendMax = double.MaxValue);
}