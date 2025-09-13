using Metasia.Editor.Models;
using Metasia.Editor.ViewModels.Inspector.Properties;

namespace Metasia.Editor.ViewModels.Inspector;

public interface IPropertyRouterViewModelFactory
{
    PropertyRouterViewModel Create(ObjectPropertyFinder.EditablePropertyInfo propertyInfo);
}
