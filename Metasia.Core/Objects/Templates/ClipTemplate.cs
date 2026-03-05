using Metasia.Core.Objects;

namespace Metasia.Core.Objects.Templates
{
    public class ClipTemplate
    {
        public List<ClipTemplateEntry> ClipEntries { get; set; } = new();

        public ClipTemplate() { }

        public ClipTemplate(IEnumerable<ClipTemplateEntry> entries)
        {
            ClipEntries = entries?.ToList() ?? Enumerable.Empty<ClipTemplateEntry>().ToList();
        }
    }

    public class ClipTemplateEntry
    {
        public int LayerIndex { get; set; }
        public int FrameOffset { get; set; }
        public string ClipXml { get; set; } = string.Empty;
        public string ClipTypeName { get; set; } = string.Empty;

        public ClipTemplateEntry() { }

        public ClipTemplateEntry(int layerIndex, int frameOffset, string clipXml, string clipTypeName)
        {
            LayerIndex = layerIndex;
            FrameOffset = frameOffset;
            ClipXml = clipXml;
            ClipTypeName = clipTypeName;
        }
    }
}