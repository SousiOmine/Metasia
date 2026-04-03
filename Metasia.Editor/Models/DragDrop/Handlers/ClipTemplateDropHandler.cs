using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Templates;
using Metasia.Core.Xml;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.DragDrop.Handlers
{
    public class ClipTemplateDropHandler : IDropHandler
    {
        public int Priority => 5;

        public bool CanHandle(IDataObject data, DropTargetContext context)
        {
            var files = data.GetFiles();
            if (files == null || !files.Any()) return false;

            var file = files.First() as IStorageFile;
            if (file == null) return false;

            var ext = Path.GetExtension(file.Name)?.ToLowerInvariant();
            return ext == ".mtmp";
        }

        public DropPreviewResult HandleDragOver(IDataObject data, DropTargetContext context)
        {
            if (!CanHandle(data, context))
            {
                return DropPreviewResult.None;
            }

            return DropPreviewResult.Copy(null);
        }

        public IEditCommand? HandleDrop(IDataObject data, DropTargetContext context)
        {
            var files = data.GetFiles();
            if (files == null || !files.Any()) return null;

            var file = files.First() as IStorageFile;
            if (file == null) return null;

            var ext = Path.GetExtension(file.Name)?.ToLowerInvariant();
            if (ext != ".mtmp") return null;

            string filePath = file.Path.LocalPath;
            if (!File.Exists(filePath)) return null;

            ClipTemplate template;
            try
            {
                template = ClipTemplateSerializer.LoadFromFile(filePath);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Failed to load clip template: {ex.Message}");
                return null;
            }

            int baseLayerIndex = context.Timeline.Layers.IndexOf(context.TargetLayer);
            if (baseLayerIndex < 0) baseLayerIndex = 0;

            var clipsToAdd = ClipTemplateSerializer.InstantiateClips(
                template,
                context.TargetFrame,
                baseLayerIndex,
                context.Timeline
            );

            return new AddClipsFromTemplateCommand(context.Timeline, clipsToAdd);
        }
    }
}