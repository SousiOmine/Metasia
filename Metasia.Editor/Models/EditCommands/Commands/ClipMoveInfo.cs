using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    /// <summary>
    /// クリップをどのレイヤーからどのレイヤーへ移動するか、どのフレームに移動するかをMoveClipsCommandに渡すためのクラス
    /// </summary>
    public class ClipMoveInfo
    {
        public MetasiaObject TargetObject { get; }
        public LayerObject SourceLayer { get; }
        public LayerObject TargetLayer { get; }
        public int OldStartFrame { get; }
        public int OldEndFrame { get; }
        public int NewStartFrame { get; }
        public int NewEndFrame { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipMoveInfo"/> class with details for moving a clip between layers and frame ranges.
        /// </summary>
        /// <param name="targetObject">The clip or object to be moved.</param>
        /// <param name="sourceLayer">The original layer from which the clip is moved.</param>
        /// <param name="targetLayer">The destination layer to which the clip is moved.</param>
        /// <param name="oldStartFrame">The starting frame of the clip in the source layer.</param>
        /// <param name="oldEndFrame">The ending frame of the clip in the source layer.</param>
        /// <param name="newStartFrame">The starting frame of the clip in the target layer.</param>
        /// <param name="newEndFrame">The ending frame of the clip in the target layer.</param>
        public ClipMoveInfo(
            MetasiaObject targetObject,
            LayerObject sourceLayer,
            LayerObject targetLayer,
            int oldStartFrame, int oldEndFrame,
            int newStartFrame, int newEndFrame)
        {
            TargetObject = targetObject;
            SourceLayer = sourceLayer;
            TargetLayer = targetLayer;
            OldStartFrame = oldStartFrame;
            OldEndFrame = oldEndFrame;
            NewStartFrame = newStartFrame;
            NewEndFrame = newEndFrame;
        }
    }
}
