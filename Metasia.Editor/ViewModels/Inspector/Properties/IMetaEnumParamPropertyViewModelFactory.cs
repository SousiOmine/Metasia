using Metasia.Editor.Models;
using Metasia.Editor.Models.States;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMetaEnumParamPropertyViewModelFactory
{
    MetaEnumParamPropertyViewModel Create(string propertyIdentifier, MetaEnumParam target);
}
