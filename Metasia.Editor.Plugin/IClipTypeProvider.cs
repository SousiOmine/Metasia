using System;
using System.Collections.Generic;

namespace Metasia.Editor.Plugin;

public interface IClipTypeProvider : IEditorPlugin
{
    IEnumerable<Type> GetClipTypes();
}
