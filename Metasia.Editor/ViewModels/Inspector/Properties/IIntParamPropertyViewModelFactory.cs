using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IIntParamPropertyViewModelFactory
{
    IntParamPropertyViewModel Create(string propertyIdentifier, MetaIntParam target, int min = int.MinValue, int max = int.MaxValue, int recommendMin = int.MinValue, int recommendMax = int.MaxValue, bool allowMultiClipApply = true, IMetasiaObject? owner = null);
}
