using System;
using System.Collections.Generic;

namespace Metasia.Editor.Plugin;

public interface IAudioEffectTypeProvider : IEditorPlugin
{
    IEnumerable<Type> GetAudioEffectTypes();
}
