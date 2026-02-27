using System;

namespace Metasia.Editor.ViewModels.Inspector.Properties;

public interface IBoolPropertyViewModelFactory
{
    BoolPropertyViewModel Create(string propertyIdentifier, bool target);
}