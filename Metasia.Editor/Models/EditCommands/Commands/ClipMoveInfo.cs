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
