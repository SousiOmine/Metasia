namespace Metasia.Editor.Services
{
    public class ClipboardService : IClipboardService
    {
        private string? _storedClipTemplateXml;

        public bool HasClips => !string.IsNullOrEmpty(_storedClipTemplateXml);

        public void StoreClips(string clipTemplateXml)
        {
            _storedClipTemplateXml = clipTemplateXml;
        }

        public string? GetStoredClips()
        {
            return _storedClipTemplateXml;
        }

        public void Clear()
        {
            _storedClipTemplateXml = null;
        }
    }
}