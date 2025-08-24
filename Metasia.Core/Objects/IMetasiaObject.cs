using System.Text.Json.Serialization;

namespace Metasia.Core.Objects
{
    public interface IMetasiaObject
    {
        string Id { get; set; }

        bool IsActive { get; set; }
    }
}