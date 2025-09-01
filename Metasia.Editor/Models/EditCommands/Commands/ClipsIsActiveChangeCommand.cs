using Metasia.Core.Objects;
using System.Collections.Generic;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class ClipsIsActiveChangeCommand : IEditCommand
    {
        public string Description => "クリップの選択状態変更";

        private readonly IEnumerable<ClipObject> _targetClips;
        private readonly bool _afterActive;
        private readonly Dictionary<ClipObject, bool> _beforeActiveStates;

        /// <summary>
        /// 複数のクリップの選択状態を変更するコマンド
        /// </summary>
        /// <param name="targetClips">選択状態を変更したいクリップのコレクション</param>
        /// <param name="isActive">変更後の選択状態</param>
        public ClipsIsActiveChangeCommand(IEnumerable<ClipObject> targetClips, bool isActive)
        {
            _targetClips = targetClips;
            _afterActive = isActive;
            _beforeActiveStates = new Dictionary<ClipObject, bool>();

            // 変更前の状態を保存
            foreach (var clip in _targetClips)
            {
                _beforeActiveStates[clip] = clip.IsActive;
            }
        }

        public void Execute()
        {
            foreach (var clip in _targetClips)
            {
                clip.IsActive = _afterActive;
            }
        }

        public void Undo()
        {
            foreach (var clip in _targetClips)
            {
                if (_beforeActiveStates.TryGetValue(clip, out var beforeActive))
                {
                    clip.IsActive = beforeActive;
                }
            }
        }
    }
}