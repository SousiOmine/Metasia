using System;
using System.Collections.Generic;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Models.Interactor
{
    /// <summary>
    /// クリップ操作に関するビジネスロジックを集約するInteractor
    /// </summary>
    public static class ClipInteractor
    {
        #region スナッピング関連

        /// <summary>
        /// 指定フレームに最も近いスナップポイントを取得する
        /// </summary>
        /// <param name="proposedFrame">提案するフレーム位置</param>
        /// <param name="thresholdFrames">スナップ閾値（フレーム単位）</param>
        /// <param name="excludeClips">スナップ対象から除外するクリップ</param>
        /// <param name="timeline">タイムラインオブジェクト</param>
        /// <returns>スナップ後のフレーム位置（スナップしなければproposedFrameをそのまま返す）</returns>
        public static int GetNearestSnapFrame(
            int proposedFrame,
            int thresholdFrames,
            IEnumerable<ClipObject> excludeClips,
            TimelineObject timeline)
        {
            var excludeIds = new HashSet<string>(excludeClips.Select(c => c.Id));

            int nearestFrame = proposedFrame;
            int minDiff = int.MaxValue;

            foreach (var layer in timeline.Layers)
            {
                foreach (var clip in layer.Objects)
                {
                    if (excludeIds.Contains(clip.Id)) continue;

                    // クリップの開始フレームとのスナップ
                    int diffStart = Math.Abs(clip.StartFrame - proposedFrame);
                    if (diffStart <= thresholdFrames && diffStart < minDiff)
                    {
                        minDiff = diffStart;
                        nearestFrame = clip.StartFrame;
                    }

                    // クリップの終了フレーム+1（次の開始位置）とのスナップ
                    int endBoundary = clip.EndFrame + 1;
                    int diffEnd = Math.Abs(endBoundary - proposedFrame);
                    if (diffEnd <= thresholdFrames && diffEnd < minDiff)
                    {
                        minDiff = diffEnd;
                        nearestFrame = endBoundary;
                    }
                }
            }

            return nearestFrame;
        }

        /// <summary>
        /// クリップ移動操作にスナッピングを適用する
        /// </summary>
        /// <param name="dropInfo">ドロップ情報（DropPositionFrameが更新される）</param>
        /// <param name="selectedClips">選択中のクリップ</param>
        /// <param name="timeline">タイムラインオブジェクト</param>
        /// <param name="framePerDip">1ピクセルあたりのフレーム数</param>
        public static void ApplyMoveSnapping(
            ClipsDropTargetContext dropInfo,
            IEnumerable<ClipObject> selectedClips,
            TimelineObject timeline,
            double framePerDip)
        {
            // マウス位置から基準クリップの提案開始フレームを計算
            int proposedStartFrame = dropInfo.DropPositionFrame - dropInfo.DraggingFrameOffsetX;
            int currentDelta = proposedStartFrame - dropInfo.ReferenceClipVM.TargetObject.StartFrame;

            var selectedClipList = selectedClips.OfType<ClipObject>().ToList();

            // スナップ閾値をフレーム単位に変換
            const double SNAP_THRESHOLD_PX = 10.0;
            int snapThresholdFrame = Math.Max(1, (int)(SNAP_THRESHOLD_PX / framePerDip));

            int bestSnapDiff = 0;
            int minAbsDiff = int.MaxValue;

            // 全選択クリップに対して最適なスナップポイントを探索
            foreach (var clip in selectedClipList)
            {
                // 現在のマウス移動量を適用した仮位置
                int tentativeStart = clip.StartFrame + currentDelta;
                int tentativeEndBoundary = clip.EndFrame + currentDelta + 1;

                // 始端のスナップ
                int snappedStart = GetNearestSnapFrame(tentativeStart, snapThresholdFrame, selectedClipList, timeline);
                if (snappedStart != tentativeStart)
                {
                    int diff = snappedStart - tentativeStart;
                    if (Math.Abs(diff) < minAbsDiff)
                    {
                        minAbsDiff = Math.Abs(diff);
                        bestSnapDiff = diff;
                    }
                }

                // 終端のスナップ
                int snappedEndBoundary = GetNearestSnapFrame(tentativeEndBoundary, snapThresholdFrame, selectedClipList, timeline);
                if (snappedEndBoundary != tentativeEndBoundary)
                {
                    int diff = snappedEndBoundary - tentativeEndBoundary;
                    if (Math.Abs(diff) < minAbsDiff)
                    {
                        minAbsDiff = Math.Abs(diff);
                        bestSnapDiff = diff;
                    }
                }
            }

            // スナップを適用した最終的な移動量
            int finalDelta = currentDelta + bestSnapDiff;

            // 制約: どのクリップも0未満にならないように
            if (selectedClipList.Any())
            {
                int minStart = selectedClipList.Min(c => c.StartFrame);
                if (finalDelta < -minStart)
                {
                    finalDelta = -minStart;
                }
            }

            // DropPositionFrameを逆算
            int finalStartFrame = dropInfo.ReferenceClipVM.TargetObject.StartFrame + finalDelta;
            dropInfo.DropPositionFrame = finalStartFrame + dropInfo.DraggingFrameOffsetX;
        }

        /// <summary>
        /// クリップリサイズ操作にスナッピングを適用して新しいフレーム範囲を返す
        /// </summary>
        /// <param name="clip">対象クリップ</param>
        /// <param name="handleName">"StartHandle" または "EndHandle"</param>
        /// <param name="originalStart">元の開始フレーム</param>
        /// <param name="originalEnd">元の終了フレーム</param>
        /// <param name="deltaPixels">マウス移動量（ピクセル）</param>
        /// <param name="timeline">タイムラインオブジェクト</param>
        /// <param name="framePerDip">1ピクセルあたりのフレーム数</param>
        /// <returns>スナップ適用後の(NewStart, NewEnd)</returns>
        public static (int NewStart, int NewEnd) ApplyResizeSnapping(
            ClipObject clip,
            string handleName,
            int originalStart,
            int originalEnd,
            double deltaPixels,
            TimelineObject timeline,
            double framePerDip)
        {
            int frameChange = (int)Math.Round(deltaPixels / framePerDip);

            int newStart = originalStart;
            int newEnd = originalEnd;

            // スナップ閾値
            const double SNAP_THRESHOLD_PX = 10.0;
            int snapThresholdFrame = Math.Max(1, (int)(SNAP_THRESHOLD_PX / framePerDip));

            var excludeClips = new[] { clip };

            if (handleName == "StartHandle")
            {
                int proposedStart = originalStart + frameChange;
                proposedStart = GetNearestSnapFrame(proposedStart, snapThresholdFrame, excludeClips, timeline);

                newStart = proposedStart;
                // 終端を超えない、かつ長さが1未満にならないように制限
                newStart = Math.Min(newStart, originalEnd - 1);
                newStart = Math.Max(newStart, 0);
            }
            else if (handleName == "EndHandle")
            {
                int proposedEnd = originalEnd + frameChange;
                // EndFrame+1（次の開始位置）でスナップ判定
                int proposedBoundary = proposedEnd + 1;
                int snappedBoundary = GetNearestSnapFrame(proposedBoundary, snapThresholdFrame, excludeClips, timeline);

                newEnd = snappedBoundary - 1;
                // 始端を下回らない、かつ長さが1未満にならないように制限
                newEnd = Math.Max(newEnd, originalStart + 1);
            }

            return (newStart, newEnd);
        }

        #endregion

        #region コマンド作成

        /// <summary>
        /// クリップ移動コマンドを作成する
        /// </summary>
        public static IEditCommand? CreateMoveClipsCommand(
            ClipsDropTargetContext dropInfo,
            TimelineObject timeline,
            LayerObject targetLayer,
            IEnumerable<ClipObject> targetObjects)
        {
            // クリップの新しい開始位置を計算
            int newStartFrame = dropInfo.DropPositionFrame - dropInfo.DraggingFrameOffsetX;
            int originalStartFrame = dropInfo.ReferenceClipVM.TargetObject.StartFrame;
            int moveFrame = newStartFrame - originalStartFrame;

            // 基準クリップのレイヤーを特定
            var referenceClip = dropInfo.ReferenceClipVM.TargetObject;
            var referenceLayer = FindOwnerLayer(timeline, referenceClip);
            if (referenceLayer is null)
            {
                return null;
            }

            // レイヤー間の移動オフセットを計算
            int sourceLayerIndex = timeline.Layers.IndexOf(referenceLayer);
            int targetLayerIndex = timeline.Layers.IndexOf(targetLayer);
            int moveLayerCount = targetLayerIndex - sourceLayerIndex;

            // 移動情報リストを構築
            var moveInfos = BuildMoveInfoList(targetObjects, timeline, moveFrame, moveLayerCount);
            if (moveInfos.Count == 0)
            {
                return null;
            }

            // 移動が有効かチェック
            if (IsMoveValid(moveInfos))
            {
                return new MoveClipsCommand(moveInfos);
            }

            // 重複を解決した移動量を取得
            int resolvedMoveFrame = ResolveValidMoveFrame(moveFrame, targetObjects, timeline, moveLayerCount);
            if (resolvedMoveFrame == moveFrame)
            {
                return null;
            }

            // 解決後の移動量で再構築
            moveInfos = BuildMoveInfoList(targetObjects, timeline, resolvedMoveFrame, moveLayerCount);
            if (moveInfos.Count > 0 && IsMoveValid(moveInfos))
            {
                return new MoveClipsCommand(moveInfos);
            }

            return null;
        }

        /// <summary>
        /// クリップリサイズコマンドを作成する
        /// </summary>
        public static IEditCommand CreateResizeCommand(
            ClipObject clip,
            int oldStart, int newStart,
            int oldEnd, int newEnd)
        {
            return new ClipResizeCommand(clip, oldStart, newStart, oldEnd, newEnd);
        }

        #endregion

        #region 検証関連

        /// <summary>
        /// クリップのリサイズが可能かどうかを検証する
        /// </summary>
        public static bool CanResize(
            ClipObject clip,
            int newStart,
            int newEnd,
            LayerObject layer)
        {
            // 基本的な検証
            if (newStart < 0) return false;
            if (newEnd <= newStart) return false;

            // 他のクリップとの重複チェック
            foreach (var other in layer.Objects)
            {
                if (other.Id == clip.Id) continue;

                // 重複判定: newStart <= other.EndFrame && newEnd >= other.StartFrame
                if (newStart <= other.EndFrame && newEnd >= other.StartFrame)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region ヘルパーメソッド

        /// <summary>
        /// クリップが所属するレイヤーを検索する
        /// </summary>
        public static LayerObject? FindOwnerLayer(TimelineObject timeline, ClipObject clip)
        {
            foreach (var layer in timeline.Layers)
            {
                if (layer.Objects.Any(o => o.Id == clip.Id))
                {
                    return layer;
                }
            }
            return null;
        }

        /// <summary>
        /// 移動情報リストを構築する
        /// </summary>
        private static List<ClipMoveInfo> BuildMoveInfoList(
            IEnumerable<ClipObject> targetObjects,
            TimelineObject timeline,
            int moveFrame,
            int moveLayerCount)
        {
            var moveInfos = new List<ClipMoveInfo>();

            foreach (var targetObject in targetObjects)
            {
                var sourceLayer = FindOwnerLayer(timeline, targetObject);
                if (sourceLayer is null) continue;

                var newLayer = GetLayerByOffset(timeline, sourceLayer, moveLayerCount);
                if (newLayer is null) continue;

                moveInfos.Add(new ClipMoveInfo(
                    targetObject,
                    sourceLayer,
                    newLayer,
                    targetObject.StartFrame,
                    targetObject.EndFrame,
                    targetObject.StartFrame + moveFrame,
                    targetObject.EndFrame + moveFrame
                ));
            }

            return moveInfos;
        }

        /// <summary>
        /// オフセットを適用したレイヤーを取得する
        /// </summary>
        private static LayerObject? GetLayerByOffset(TimelineObject timeline, LayerObject currentLayer, int offset)
        {
            if (timeline?.Layers is null) return null;

            int currentIndex = timeline.Layers.IndexOf(currentLayer);
            int newIndex = currentIndex + offset;

            if (newIndex < 0 || newIndex >= timeline.Layers.Count) return null;

            return timeline.Layers[newIndex];
        }

        /// <summary>
        /// クリップ移動後に重複が発生しないか検証する
        /// </summary>
        private static bool IsMoveValid(IEnumerable<ClipMoveInfo> moveInfos)
        {
            var movingClipIds = new HashSet<string>(moveInfos.Select(x => x.TargetObject.Id));

            foreach (var group in moveInfos.GroupBy(x => x.TargetLayer))
            {
                // 既存クリップ（移動対象を除外）の区間
                var ranges = group.Key.Objects
                    .Where(o => !movingClipIds.Contains(o.Id))
                    .Select(o => (o.StartFrame, o.EndFrame))
                    .ToList();

                // 移動後クリップの区間を追加
                foreach (var info in group)
                {
                    if (info.NewStartFrame < 0 || info.NewStartFrame > info.NewEndFrame)
                    {
                        return false;
                    }
                    ranges.Add((info.NewStartFrame, info.NewEndFrame));
                }

                // 始端でソートして隣接比較で重複判定
                var ordered = ranges.OrderBy(r => r.StartFrame).ToList();
                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].StartFrame <= ordered[i - 1].EndFrame)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 重複を解決した有効な移動フレーム量を取得する
        /// </summary>
        private static int ResolveValidMoveFrame(
            int originalMoveFrame,
            IEnumerable<ClipObject> targetObjects,
            TimelineObject timeline,
            int moveLayerCount)
        {
            var forbiddenIntervals = BuildForbiddenIntervals(targetObjects, timeline, moveLayerCount);

            if (forbiddenIntervals.Count == 0) return originalMoveFrame;

            var merged = MergeIntervals(forbiddenIntervals);

            // 元の移動量が有効かチェック
            bool isRestricted = merged.Any(interval =>
                originalMoveFrame >= interval.Start && originalMoveFrame <= interval.End);

            if (!isRestricted) return originalMoveFrame;

            // 最も近い有効なフレームを探索
            return FindClosestValidFrame(originalMoveFrame, merged);
        }

        /// <summary>
        /// 禁止区間のリストを構築する
        /// </summary>
        private static List<(long Start, long End)> BuildForbiddenIntervals(
            IEnumerable<ClipObject> targetObjects,
            TimelineObject timeline,
            int moveLayerCount)
        {
            var forbiddenIntervals = new List<(long Start, long End)>();
            var targetObjectIds = new HashSet<string>(targetObjects.Select(x => x.Id));

            foreach (var targetObject in targetObjects)
            {
                var sourceLayer = FindOwnerLayer(timeline, targetObject);
                if (sourceLayer is null) continue;

                var newLayer = GetLayerByOffset(timeline, sourceLayer, moveLayerCount);
                if (newLayer is null) continue;

                // 開始フレームが0以上である制約
                forbiddenIntervals.Add((long.MinValue, -targetObject.StartFrame - 1));

                // 他のクリップとの衝突を禁止区間として追加
                foreach (var stationary in newLayer.Objects)
                {
                    if (targetObjectIds.Contains(stationary.Id)) continue;

                    long lower = (long)stationary.StartFrame - targetObject.EndFrame;
                    long upper = (long)stationary.EndFrame - targetObject.StartFrame;

                    forbiddenIntervals.Add((lower, upper));
                }
            }

            return forbiddenIntervals;
        }

        /// <summary>
        /// 区間をマージする
        /// </summary>
        private static List<(long Start, long End)> MergeIntervals(List<(long Start, long End)> intervals)
        {
            if (intervals.Count == 0) return new List<(long Start, long End)>();

            var sorted = intervals.OrderBy(x => x.Start).ToList();
            var merged = new List<(long Start, long End)>();

            var current = sorted[0];
            for (int i = 1; i < sorted.Count; i++)
            {
                var next = sorted[i];
                if (next.Start <= current.End + 1)
                {
                    current.End = Math.Max(current.End, next.End);
                }
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }
            merged.Add(current);

            return merged;
        }

        /// <summary>
        /// 最も近い有効なフレームを探索する
        /// </summary>
        private static int FindClosestValidFrame(int proposedFrame, List<(long Start, long End)> merged)
        {
            long closestDiff = long.MaxValue;
            int closestFrame = proposedFrame;

            foreach (var interval in merged)
            {
                if (interval.Start > long.MinValue)
                {
                    long c1 = interval.Start - 1;
                    if (Math.Abs(c1 - proposedFrame) < closestDiff)
                    {
                        closestDiff = Math.Abs(c1 - proposedFrame);
                        closestFrame = (int)c1;
                    }
                }

                if (interval.End < long.MaxValue)
                {
                    long c2 = interval.End + 1;
                    if (Math.Abs(c2 - proposedFrame) < closestDiff)
                    {
                        closestDiff = Math.Abs(c2 - proposedFrame);
                        closestFrame = (int)c2;
                    }
                }
            }

            return closestFrame;
        }

        #endregion
    }
}
