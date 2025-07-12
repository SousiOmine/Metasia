using Avalonia;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Layout;
using Metasia.Editor.Models.EditCommands.Commands;
using System.Diagnostics;

namespace Metasia.Editor.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        /// <summary>
        /// 表示するタイムラインオブジェクト
        /// </summary>
        public TimelineObject Timeline
        {
            get => _timeline;
            set => this.RaiseAndSetIfChanged(ref _timeline, value);
        }

        /// <summary>
        /// 横方向の表示幅の倍率
        /// </summary>
        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set
            {
                this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
                ChangeFramePerDIP();
            } 
        }

        /// <summary>
        /// 左のレイヤーごとのボタンのViewModelら
        /// </summary>
        public ObservableCollection<LayerButtonViewModel> LayerButtons { get; } = new();

        /// <summary>
        /// 各レイヤーのViewModel
        /// </summary>
        public ObservableCollection<LayerCanvasViewModel> LayerCanvas { get; } = new();

        /// <summary>
        /// 現在表示しているフレーム PlayerViewModelと連動する
        /// </summary>
        public int Frame
        {
            get => PlayerViewModel.Frame;
            set => PlayerViewModel.Frame = value;
        }

        /// <summary>
        /// タイムラインのカーソルの位置
        /// </summary>
        public double CursorLeft
        {
            get => _cursorLeft;
            set => this.RaiseAndSetIfChanged(ref _cursorLeft, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;

        private double _cursorLeft;

        private readonly PlayerViewModel PlayerViewModel;

        public event EventHandler? ProjectChanged;

        public TimelineViewModel(PlayerViewModel playerViewModel)
        {
            this.PlayerViewModel = playerViewModel;

            //横方向の拡大率は初期３で固定
            Frame_Per_DIP = 3;
            _timeline = PlayerViewModel.TargetTimeline;

            //PlayerViewModel側からフレームの変更があればカーソルの描画位置を反映
            PlayerViewModel.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            // ViewPaintRequestのハンドラを設定
            PlayerViewModel.ViewPaintRequest += () =>
            {
                // タイムラインの更新が必要な場合はここで行う
                CursorLeft = PlayerViewModel.Frame * Frame_Per_DIP;
            };

            //プロジェクトに変更が加えられたときには自身のイベントも発火する
            PlayerViewModel.ProjectChanged += (sender, args) =>
            {
                ProjectChanged?.Invoke(this, EventArgs.Empty);
            };

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(this, layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, PlayerViewModel, layer));
            }
        }

        public bool RunEditCommand(IEditCommand command)
        {
            return PlayerViewModel.RunEditCommand(command);
        }

        public void SetFrameFromPosition(double position)
        {
            Frame = (int)(position / Frame_Per_DIP);
        }

        public void ClipSelect(MetasiaObject obj, bool isMultiSelect = false)
        {
            if (isMultiSelect)
            {
                // 複数選択モード：既に選択されている場合は選択解除、そうでなければ追加
                if (PlayerViewModel.SelectingObjects.Contains(obj))
                {
                    PlayerViewModel.SelectingObjects.Remove(obj);
                }
                else
                {
                    PlayerViewModel.SelectingObjects.Add(obj);
                }
            }
            else
            {
                // 単一選択モード：既存の選択をクリアして新しいクリップを選択
                PlayerViewModel.SelectingObjects.Clear();
                PlayerViewModel.SelectingObjects.Add(obj);
            }
        }

        public bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                return ownerLayer.CanPlaceObjectAt(clipObject, newStartFrame, newEndFrame);
            }
            return false;
        }

        public void ClipsDropped(int moveFrame, int moveLayerCount)
        {
            //選択中のオブジェクトすべてを対象とする
            List<MetasiaObject> targetObjects = new();
            foreach (var metasiaObject in PlayerViewModel.SelectingObjects)
            {
                targetObjects.Add(metasiaObject);
            }

            // 移動可能かを確認
            // foreach (var targetObject in targetObjects)
            // {
            //     var sourceLayer = FindOwnerLayer(targetObject);
            //     if (sourceLayer is null) continue;

            //     // 移動先のレイヤーが存在しなければ終了
            //     var newLayer = GetLayerByOffset(sourceLayer, moveLayerCount);
            //     if (newLayer is null) return;

            //     // 移動先のレイヤーと位置に配置可能であれば終了
            //     if (!newLayer.CanPlaceObjectAt(targetObject, targetObject.StartFrame + moveFrame, targetObject.EndFrame + moveFrame)) return;
            // }

            List<ClipMoveInfo> moveInfos = new();
            foreach (var targetObject in targetObjects)
            {
                var sourceLayer = FindOwnerLayer(targetObject);
                if (sourceLayer is null) continue;

                var newLayer = GetLayerByOffset(sourceLayer, moveLayerCount);
                if (newLayer is null) continue;

                moveInfos.Add(new ClipMoveInfo(targetObject, sourceLayer, newLayer, targetObject.StartFrame, targetObject.EndFrame, targetObject.StartFrame + moveFrame, targetObject.EndFrame + moveFrame));

            }

            if (moveInfos.Count > 0)
            {
                var command = new MoveClipsCommand(moveInfos);
                RunEditCommand(command);
            }
        }

        private LayerObject? FindOwnerLayer(MetasiaObject targetObject)
        {
            foreach (var layer in Timeline.Layers)
            {
                if (layer.Objects.Any(x => x.Id == targetObject.Id))
                {
                    return layer;
                }
            }
            return null;
        }

        private LayerObject? GetLayerByOffset(LayerObject currentLayer, int offset)
        {
            if (Timeline?.Layers is null) return null;
            
            int currentIndex = Timeline.Layers.IndexOf(currentLayer);
            int newIndex = currentIndex + offset;

            if (newIndex < 0 || newIndex >= Timeline.Layers.Count) return null;

            return Timeline.Layers[newIndex];
        }

        
        
        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * Frame_Per_DIP;
        }
    }
}
