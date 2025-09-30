using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.Interactor
{
    public static class TimelineInteractor
    {
        public static IEditCommand? CreateMoveClipsCommand(ClipsDropTargetContext dropInfo, TimelineObject timeline, LayerObject referencedTargetLayer, IEnumerable<ClipObject> targetObjects)
        {

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
            ClipObject? referencedClipObject = dropInfo.ReferenceClipVM.TargetObject;
            LayerObject? referencedObjectLayer = null;
            foreach (var layer in timeline.Layers)
            {
                if (layer.Objects.Any(x => x.Id == referencedClipObject.Id))
                {
                    referencedObjectLayer = layer;
                    break;
                }
            }
            if (referencedObjectLayer is null)
            {
                return null;
            }

            int sourceLayerIndex = timeline.Layers.IndexOf(referencedObjectLayer);
            int targetLayerIndex = timeline.Layers.IndexOf(referencedTargetLayer);
            int moveLayerCount = targetLayerIndex - sourceLayerIndex;

            List<ClipMoveInfo> moveInfos = new();
            foreach (var targetObject in targetObjects)
            {
                var sourceLayer = FindOwnerLayer(timeline, targetObject);
                if (sourceLayer is null) continue;

                var newLayer = GetLayerByOffset(sourceLayer, moveLayerCount);
                if (newLayer is null) continue;

                moveInfos.Add(new ClipMoveInfo(targetObject, sourceLayer, newLayer, targetObject.StartFrame, targetObject.EndFrame, targetObject.StartFrame + moveFrame, targetObject.EndFrame + moveFrame));
            }
            if(moveInfos.Count > 0)
            {
                return new MoveClipsCommand(moveInfos);
            }
            return null;
        }

        public static IEditCommand? CreateCoordPointsValueChangeCommand(string propertyIdentifier, CoordPoint targetCoordPoint, double beforeValue, double afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<CoordPointsValueChangeCommand.CoordPointValueChangeInfo> changeInfos = new();
            foreach(var clip in selectedClips)
            {
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if(property is null || property.PropertyValue!.GetType() != typeof(MetaNumberParam<double>)) continue;
                var coordPoints = (MetaNumberParam<double>)property.PropertyValue!;
                var coordPoint = coordPoints.Params.FirstOrDefault(x => x.Id == targetCoordPoint.Id);
                if (coordPoint is not null)
                {
                    var valueDifference = afterValue - beforeValue;
                    changeInfos.Add(new CoordPointsValueChangeCommand.CoordPointValueChangeInfo(coordPoints, coordPoint, valueDifference));
                }
                else if(coordPoint is null && coordPoints.Params.Count == 1)
                {
                    coordPoint = coordPoints.Params.First();
                    var valueDifference = afterValue - beforeValue;
                    changeInfos.Add(new CoordPointsValueChangeCommand.CoordPointValueChangeInfo(coordPoints, coordPoint, valueDifference));
                }
            }
            return new CoordPointsValueChangeCommand(changeInfos);
        }

        public static IEditCommand? CreateStringValueChangeCommand(string propertyIdentifier, string beforeValue, string afterValue, IEnumerable<ClipObject> selectedClips)
        {
            List<StringValueChangeCommand.StringValueChangeInfo> changeInfos = new();
            foreach(var clip in selectedClips)
            {
                var properties = ObjectPropertyFinder.FindEditableProperties(clip);
                var property = properties.FirstOrDefault(x => x.Identifier == propertyIdentifier);
                if(property is null || property.PropertyValue is not string) continue;
                
                changeInfos.Add(new StringValueChangeCommand.StringValueChangeInfo(clip, propertyIdentifier, beforeValue, afterValue));
            }
            return changeInfos.Count > 0 ? new StringValueChangeCommand(changeInfos) : null;
        }

        private static LayerObject? FindOwnerLayer(TimelineObject timeline, ClipObject targetObject)
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
    }
}