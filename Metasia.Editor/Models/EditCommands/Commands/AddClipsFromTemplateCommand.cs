using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class AddClipsFromTemplateCommand : IEditCommand
    {
        public string Description => "クリップテンプレートから追加";

        private readonly List<(ClipObject clip, int layerIndex)> _clipsToAdd;
        private readonly TimelineObject _timeline;
        private readonly List<string> _createdLayerIds = new();

        public AddClipsFromTemplateCommand(TimelineObject timeline, List<(ClipObject clip, int layerIndex)> clipsToAdd)
        {
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            _clipsToAdd = clipsToAdd ?? throw new ArgumentNullException(nameof(clipsToAdd));
        }

        public void Execute()
        {
            foreach (var (clip, layerIndex) in _clipsToAdd)
            {
                while (_timeline.Layers.Count <= layerIndex)
                {
                    var newLayer = new LayerObject
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"Layer {_timeline.Layers.Count + 1}"
                    };
                    _timeline.Layers.Add(newLayer);
                    _createdLayerIds.Add(newLayer.Id);
                }

                var targetLayer = _timeline.Layers[layerIndex];
                if (!targetLayer.Objects.Contains(clip))
                {
                    targetLayer.Objects.Add(clip);
                }
            }
        }

        public void Undo()
        {
            foreach (var (clip, layerIndex) in _clipsToAdd)
            {
                if (layerIndex < _timeline.Layers.Count)
                {
                    var layer = _timeline.Layers[layerIndex];
                    if (layer.Objects.Contains(clip))
                    {
                        layer.Objects.Remove(clip);
                    }
                }
            }

            for (int i = _createdLayerIds.Count - 1; i >= 0; i--)
            {
                var layerId = _createdLayerIds[i];
                var layerToRemove = _timeline.Layers.FirstOrDefault(l => l.Id == layerId);
                if (layerToRemove != null)
                {
                    _timeline.Layers.Remove(layerToRemove);
                }
            }
        }
    }
}