using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using System;
using System.Collections.Generic;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class ClipsAddCommand : IEditCommand
    {
        public string Description => "クリップの追加";

        private readonly List<(ClipObject clip, LayerObject layer)> _clipsToAdd;
        private readonly List<(ClipObject clip, LayerObject layer)> _addedClips;

        public ClipsAddCommand(IEnumerable<(ClipObject clip, LayerObject layer)> clipsToAdd)
        {
            ArgumentNullException.ThrowIfNull(clipsToAdd);

            _clipsToAdd = new List<(ClipObject clip, LayerObject layer)>();
            _addedClips = new List<(ClipObject clip, LayerObject layer)>();

            foreach (var (clip, layer) in clipsToAdd)
            {
                ArgumentNullException.ThrowIfNull(clip, nameof(clipsToAdd));
                ArgumentNullException.ThrowIfNull(layer, nameof(clipsToAdd));
                _clipsToAdd.Add((clip, layer));
            }
        }

        public void Execute()
        {
            _addedClips.Clear();

            foreach (var (clip, layer) in _clipsToAdd)
            {
                layer.Objects.Add(clip);
                _addedClips.Add((clip, layer));
            }
        }

        public void Undo()
        {
            for (int i = _addedClips.Count - 1; i >= 0; i--)
            {
                var (clip, layer) = _addedClips[i];
                layer.Objects.Remove(clip);
            }
            _addedClips.Clear();
        }
    }
}