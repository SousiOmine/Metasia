namespace Metasia.Editor.Services
{
    public interface IClipboardService
    {
        bool HasClips { get; }

        void StoreClips(string clipTemplateXml);

        string? GetStoredClips();

        void Clear();
    }
}