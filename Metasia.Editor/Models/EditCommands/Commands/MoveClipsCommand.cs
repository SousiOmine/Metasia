using System.Collections;
using System.Collections.Generic;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class MoveClipsCommand : IEditCommand
    {
        public string Description => "クリップの移動";

        private readonly IEnumerable<ClipMoveInfo> _moveInfos;

        public MoveClipsCommand(IEnumerable<ClipMoveInfo> moveInfos)
        {
            _moveInfos = moveInfos;
        }

        public void Execute()
        {
            foreach (var moveInfo in _moveInfos)
            {
                if (moveInfo.SourceLayer != moveInfo.TargetLayer)
                {
                    moveInfo.SourceLayer.Objects.Remove(moveInfo.TargetObject);
                }

                moveInfo.TargetObject.StartFrame = moveInfo.NewStartFrame;
                moveInfo.TargetObject.EndFrame = moveInfo.NewEndFrame;

                if (moveInfo.SourceLayer != moveInfo.TargetLayer)
                {
                    moveInfo.TargetLayer.Objects.Add(moveInfo.TargetObject);
                }

            }
        }

        public void Undo()
        {
            foreach (var moveInfo in _moveInfos)
            {
                if (moveInfo.SourceLayer != moveInfo.TargetLayer)
                {
                    moveInfo.TargetLayer.Objects.Remove(moveInfo.TargetObject);
                }

                moveInfo.TargetObject.StartFrame = moveInfo.OldStartFrame;
                moveInfo.TargetObject.EndFrame = moveInfo.OldEndFrame;

                if (moveInfo.SourceLayer != moveInfo.TargetLayer)
                {
                    moveInfo.SourceLayer.Objects.Add(moveInfo.TargetObject);
                }
            }
        }
    }
}