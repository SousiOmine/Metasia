namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IColorPropertyViewModelFactory
{
    ColorPropertyViewModel Create(string propertyIdentifier, Metasia.Core.Objects.Parameters.Color.ColorRgb8 target);
}
