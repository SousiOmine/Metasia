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
        private PlayerViewModel playerViewModel;
        public LayerObject TargetLayer { get; private set; }

        private double _frame_per_DIP;
        private double width;

        public LayerCanvasViewModel(TimelineViewModel parentTimeline, PlayerViewModel playerViewModel, LayerObject targetLayer) 
        {
            this.parentTimeline = parentTimeline;
            this.playerViewModel = playerViewModel;
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

            // SelectingObjectsの変更を監視してIsSelectingを更新
            playerViewModel.SelectingObjects.CollectionChanged += (sender, args) =>
            {
                ResetSelectedClip();
                foreach (var obj in playerViewModel.SelectingObjects)
                {
                    var clip = ClipsAndBlanks.FirstOrDefault(c => c.TargetObject.Id == obj.Id);
                    if (clip != null)
                    {
                        clip.IsSelecting = true;
                    }
                }
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
                ClipsDropped(dropInfo);
            }
            else
            {
                Debug.WriteLine("Cannot drop at this location");
            }
        }
        
        private void ClipsDropped(ClipsDropTargetInfo dropInfo)
        {
            parentTimeline.ClipsDropped(dropInfo, TargetLayer);
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
                    if (playerViewModel.SelectingObjects.Any(x => x.Id == id))
                    {
                        clipVM.IsSelecting = true;
                    }
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
