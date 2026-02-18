using Metasia.Core.Render;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IBlendModeParamPropertyViewModelFactory
{
    BlendModeParamPropertyViewModel Create(string propertyIdentifier, BlendModeParam target);
}