using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Editor.Models.DragDropData;

namespace Metasia.Editor.ViewModels.Controls
{
    public class LayerCanvasViewModel : ViewModelBase
    {
        public ObservableCollection<ClipViewModel> ClipsAndBlanks { get; set; } = new();

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set => this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
        }

        public double Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
        }

        /// <summary>
        /// ドロップ処理のコマンド
        /// </summary>
        public ICommand HandleDropCommand { get; }

        private TimelineViewModel parentTimeline;

        public LayerObject TargetLayer { get; private set; }

        private double _frame_per_DIP;
        private double width;

        public LayerCanvasViewModel(TimelineViewModel parentTimeline, LayerObject targetLayer) 
        {
            this.parentTimeline = parentTimeline;
            this.TargetLayer = targetLayer;

            // ドロップ処理コマンドの初期化
            HandleDropCommand = ReactiveCommand.Create<ClipsDropTargetInfo>(
                execute: ExecuteHandleDrop,
                canExecute: this.WhenAnyValue(x => x.TargetLayer).Select(layer => layer != null)
            );

            parentTimeline.WhenAnyValue(x => x.Frame_Per_DIP).Subscribe
                (Frame_Per_DIP =>
                {
                    this.Frame_Per_DIP = Frame_Per_DIP;
                    ChangeFramePerDIP();
                });

            RelocateClips();

            // TimelineViewModelのProjectChangedイベントを購読
            parentTimeline.ProjectChanged += (sender, args) =>
            {
                RelocateClips();
            };
        }

        public void ResetSelectedClip()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.IsSelecting = false;
            }
        }

        /// <summary>
        /// レイヤーにあるクリップの大きさを再計算する
        /// </summary>
        public void RecalculateSize()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.RecalculateSize();
            }
        }
        /// <summary>
        /// 座標変換のためのヘルパーメソッド（ViewModelで論理座標を扱う）
        /// </summary>
        public int ConvertPositionToFrame(double positionX, double framePerDIP)
        {
            return (int)(positionX / framePerDIP);
        }

        /// <summary>
        /// タイムラインVMのドロップ処理を呼び出す
        /// </summary>
        private void ExecuteHandleDrop(ClipsDropTargetInfo dropInfo)
        {
            if (dropInfo.DragData is not null && dropInfo.CanDrop)
            {
                // クリップの新しい左端位置を計算（ドロップ位置 - クリップ内オフセット）
                double newClipLeftPosition = dropInfo.DropPositionX - dropInfo.DragData.DraggingClipOffsetX;
                int newStartFrame = ConvertPositionToFrame(newClipLeftPosition, dropInfo.DragData.FramePerDIP_AtDragStart);
                
                // 元のクリップの開始フレームと比較して移動量を算出
                int originalStartFrame = dropInfo.DragData.ReferencedClipVM.TargetObject.StartFrame;
                int moveFrame = newStartFrame - originalStartFrame;

                // クリップをレイヤー方向にどれだけ移動するか算出
                var referencedMetasiaObject = dropInfo.DragData.ReferencedClipVM.TargetObject;
                LayerObject? referencedObjectLayer = null;
                foreach (var layer in parentTimeline.Timeline.Layers)
                {
                    if (layer.Objects.Any(x => x.Id == referencedMetasiaObject.Id))
                    {
                        referencedObjectLayer = layer;
                        break;
                    }
                }
                if (referencedObjectLayer is null)
                {
                    return;
                }

                int sourceLayerIndex = parentTimeline.Timeline.Layers.IndexOf(referencedObjectLayer);
                int targetLayerIndex = parentTimeline.Timeline.Layers.IndexOf(TargetLayer);
                int moveLayerCount = targetLayerIndex - sourceLayerIndex;

                ClipsDropped(moveFrame, moveLayerCount);
            }
            else
            {
                Debug.WriteLine("Cannot drop at this location");
            }
        }
        
        private void ClipsDropped(int moveFrame, int moveLayerCount)
        {
            parentTimeline.ClipsDropped(moveFrame, moveLayerCount);
        }

        /// <summary>
        /// 配下のクリップをレイヤー配下のオブジェクトに合わせる
        /// </summary>
        private void RelocateClips()
        {
            //TargetLayer.ObjectsにあってClipsAndBlanksにないオブジェクトID
            List<string> objectIds = TargetLayer.Objects.Select(x => x.Id).ToList();
            List<string> clipIds = ClipsAndBlanks.Select(x => x.TargetObject.Id).ToList();

            var diffIds = objectIds.Except(clipIds).ToList();

            // 新しく追加されたオブジェクトをClipsAndBlanksに追加
            foreach (var id in diffIds)
            {
                var obj = TargetLayer.Objects.FirstOrDefault(x => x.Id == id);
                if (obj is not null)
                {
                    var clipVM = new ClipViewModel(obj, parentTimeline);
                    ClipsAndBlanks.Add(clipVM);
                }
            }

            // ClipsAndBlanksにあってTargetLayer.ObjectsにないオブジェクトID
            var diffIds2 = clipIds.Except(objectIds).ToList();

            // ClipsAndBlanksにあってTargetLayer.Objectsにないオブジェクトを削除
            foreach (var id in diffIds2)
            {
                var clipVM = ClipsAndBlanks.FirstOrDefault(x => x.TargetObject.Id == id);
                if (clipVM is not null)
                {
                    ClipsAndBlanks.Remove(clipVM);
                }
            }

            RecalculateSize();
            ChangeFramePerDIP();

        }

        private void ChangeFramePerDIP()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.Frame_Per_DIP = Frame_Per_DIP;
            }
            Width = TargetLayer.EndFrame * Frame_Per_DIP;
        }
    }
}
