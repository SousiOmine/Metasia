using System;
using System.Collections.Generic;

namespace Metasia.Editor.Plugin;

public interface IVisualEffectTypeProvider : IEditorPlugin
{
    IEnumerable<Type> GetVisualEffectTypes();
}
