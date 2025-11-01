using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaFontParamPropertyViewModelFactory
{
    MetaFontParamPropertyViewModel Create(string propertyIdentifier, MetaFontParam target);
}
