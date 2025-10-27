using Metasia.Core.Typography;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaFontParamPropertyViewModelFactory
{
    MetaFontParamPropertyViewModel Create(string propertyIdentifier, MetaFontParam target);
}
