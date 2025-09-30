namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IStringPropertyViewModelFactory
{
    StringPropertyViewModel Create(string propertyIdentifier, string target);
}