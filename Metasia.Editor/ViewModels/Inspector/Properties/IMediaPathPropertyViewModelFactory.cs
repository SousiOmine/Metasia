using Metasia.Core.Media;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IMediaPathPropertyViewModelFactory
{
    MediaPathPropertyViewModel Create(string propertyIdentifier, MediaPath target);
}
