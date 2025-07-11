using System.Collections;
using System.Collections.Generic;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class MoveClipsCommand : IEditCommand
    {
        public string Description => "クリップの移動";

        private readonly IEnumerable<ClipMoveInfo> _moveInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveClipsCommand"/> class with the specified collection of clip move operations.
        /// </summary>
        /// <param name="moveInfos">A collection of <see cref="ClipMoveInfo"/> objects representing the clips to move and their target positions.</param>
        public MoveClipsCommand(IEnumerable<ClipMoveInfo> moveInfos)
        {
            _moveInfos = moveInfos;
        }

        /// <summary>
        /// Moves each clip specified in the command to its target layer and updates its frame positions.
        /// </summary>
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

        /// <summary>
        /// Reverses the move operation for each clip, restoring them to their original layers and frame positions.
        /// </summary>
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