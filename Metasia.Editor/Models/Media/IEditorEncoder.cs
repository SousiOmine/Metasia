

using Metasia.Core.Encode;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Models.Media;

public interface IEditorEncoder : IEncoder
{
    string Name { get; }
    string[] SupportedExtensions { get; }
}
