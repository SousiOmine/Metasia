using System;
using System.Collections.Generic;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.Interactor
{
    public static class TimelineInteractor
    {
        public static IEditCommand CreateMoveClipsCommand(ClipsDropTargetContext dropInfo, TimelineObject timeline, LayerObject referencedTargetLayer, IEnumerable<MetasiaObject> targetObjects)
        {
            // オブジェクトを含んでいるレイヤーを探索
            LayerObject? FindOwnerLayer(MetasiaObject targetObject)
            {
                foreach (var layer in timeline.Layers)
                {
                    if (layer.Objects.Any(x => x.Id == targetObject.Id))
                    {
                        return layer;
                    }
                }
                return null;
            }

            // 任意のレイヤーを基準にそのレイヤーより指定した数だけ上下の階層のレイヤーを探索
            LayerObject? GetLayerByOffset(LayerObject currentLayer, int offset)
            {
                if (timeline?.Layers is null) return null;
            
                int currentIndex = timeline.Layers.IndexOf(currentLayer);
                int newIndex = currentIndex + offset;

                if (newIndex < 0 || newIndex >= timeline.Layers.Count) return null;

                return timeline.Layers[newIndex];
            }

            // クリップの新しい左端位置を計算（ドロップ位置 - クリップ内オフセット）
            int newStartFrame = dropInfo.DropPositionFrame - dropInfo.DraggingFrameOffsetX;
            
            // 元のクリップの開始フレームと比較して移動量を算出
            int originalStartFrame = dropInfo.ReferenceClipVM.TargetObject.StartFrame;
            int moveFrame = newStartFrame - originalStartFrame;

            // クリップをレイヤー方向にどれだけ移動するか算出
            MetasiaObject? referencedMetasiaObject = dropInfo.ReferenceClipVM.TargetObject;
            LayerObject? referencedObjectLayer = null;
            foreach (var layer in timeline.Layers)
            {
                if (layer.Objects.Any(x => x.Id == referencedMetasiaObject.Id))
                {
                    referencedObjectLayer = layer;
                    break;
                }
            }
            if (referencedObjectLayer is null)
            {
                throw new Exception("Referenced object layer not found");
            }

            int sourceLayerIndex = timeline.Layers.IndexOf(referencedObjectLayer);
            int targetLayerIndex = timeline.Layers.IndexOf(referencedTargetLayer);
            int moveLayerCount = targetLayerIndex - sourceLayerIndex;

            List<ClipMoveInfo> moveInfos = new();
            foreach (var targetObject in targetObjects)
            {
                var sourceLayer = FindOwnerLayer(targetObject);
                if (sourceLayer is null) continue;

                var newLayer = GetLayerByOffset(sourceLayer, moveLayerCount);
                if (newLayer is null) continue;

                moveInfos.Add(new ClipMoveInfo(targetObject, sourceLayer, newLayer, targetObject.StartFrame, targetObject.EndFrame, targetObject.StartFrame + moveFrame, targetObject.EndFrame + moveFrame));
            }
            if(moveInfos.Count > 0)
            {
                return new MoveClipsCommand(moveInfos);
            }
            throw new Exception("MoveClipsCommand not created");
        }
    }
}