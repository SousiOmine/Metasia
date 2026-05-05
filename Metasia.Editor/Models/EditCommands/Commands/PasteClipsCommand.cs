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
    public class PasteClipsCommand : IEditCommand
    {
        public string Description => "クリップの貼り付け";

        private readonly List<(ClipObject clip, int originalLayerIndex)> _clipsToAdd;
        private readonly TimelineObject _timeline;
        private readonly List<LayerObject> _createdLayers = new();
        private List<(ClipObject clip, LayerObject layer)> _placedClips = new();

        public IReadOnlyList<(ClipObject clip, LayerObject layer)> PlacedClips => _placedClips;

        public PasteClipsCommand(
            TimelineObject timeline,
            List<(ClipObject clip, int originalLayerIndex)> clipsToAdd)
        {
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            _clipsToAdd = clipsToAdd ?? throw new ArgumentNullException(nameof(clipsToAdd));
        }

        public void Execute()
        {
            _createdLayers.Clear();

            _placedClips = ResolvePlacements();

            foreach (var (clip, layer) in _placedClips)
            {
                if (!layer.Objects.Contains(clip))
                {
                    layer.Objects.Add(clip);
                }
            }
        }

        public void Undo()
        {
            foreach (var (clip, layer, _) in GetAddedClipsInfo())
            {
                if (layer.Objects.Contains(clip))
                {
                    layer.Objects.Remove(clip);
                }
            }

            for (int i = _createdLayers.Count - 1; i >= 0; i--)
            {
                _timeline.Layers.Remove(_createdLayers[i]);
            }
            _createdLayers.Clear();
        }

        private List<(ClipObject clip, LayerObject layer)> ResolvePlacements()
        {
            var result = new List<(ClipObject clip, LayerObject layer)>();

            foreach (var (clip, originalLayerIndex) in _clipsToAdd)
            {
                if (originalLayerIndex < 0)
                    continue;

                LayerObject? targetLayer = null;
                int searchStartIndex = Math.Min(originalLayerIndex, _timeline.Layers.Count - 1);
                if (searchStartIndex < 0)
                    searchStartIndex = 0;

                for (int i = searchStartIndex; i < _timeline.Layers.Count; i++)
                {
                    var layer = _timeline.Layers[i];
                    if (layer.CanPlaceObjectAt(clip, clip.StartFrame, clip.EndFrame))
                    {
                        targetLayer = layer;
                        break;
                    }
                }

                if (targetLayer == null)
                {
                    var newLayer = new LayerObject
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"Layer {_timeline.Layers.Count + 1}"
                    };
                    _timeline.Layers.Add(newLayer);
                    _createdLayers.Add(newLayer);
                    targetLayer = newLayer;
                }

                result.Add((clip, targetLayer));
            }

            return result;
        }

        private List<(ClipObject clip, LayerObject layer, int index)> GetAddedClipsInfo()
        {
            var result = new List<(ClipObject clip, LayerObject layer, int index)>();

            foreach (var (clip, originalLayerIndex) in _clipsToAdd)
            {
                if (originalLayerIndex < 0)
                    continue;

                foreach (var layer in _timeline.Layers)
                {
                    int index = layer.Objects.IndexOf(clip);
                    if (index >= 0)
                    {
                        result.Add((clip, layer, index));
                        break;
                    }
                }
            }

            return result;
        }
    }
}
