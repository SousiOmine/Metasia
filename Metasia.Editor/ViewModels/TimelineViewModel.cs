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
using Metasia.Editor.Services;


using Avalonia.Layout;
using Metasia.Editor.Models.EditCommands.Commands;
using System.Diagnostics;

namespace Metasia.Editor.ViewModels
{
    public class TimelineViewModel : ViewModelBase, ITimelineContext
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
        /// 選択しているクリップ
        /// </summary>
        public ObservableCollection<ClipViewModel> SelectClip { get; } = new();
        
        /// <summary>
        /// 現在表示しているフレーム PlayerViewModelと連動する
        /// </summary>
        public int Frame
        {
            get => playerViewModel.Frame;
            set => playerViewModel.Frame = value;
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

        private PlayerViewModel playerViewModel;
        private ITimelineInteractionService timelineInteractionService;

        public event EventHandler? ProjectChanged;

        public TimelineViewModel(PlayerViewModel playerViewModel)
        {
            this.playerViewModel = playerViewModel;

            //横方向の拡大率は初期３で固定
            Frame_Per_DIP = 3;
            _timeline = playerViewModel.TargetTimeline;

            // Initialize the interaction service
            timelineInteractionService = new TimelineInteractionService(this);

            //PlayerViewModel側からフレームの変更があればカーソルの描画位置を反映
            playerViewModel.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            // ViewPaintRequestのハンドラを設定
            playerViewModel.ViewPaintRequest += () =>
            {
                // タイムラインの更新が必要な場合はここで行う
                CursorLeft = playerViewModel.Frame * Frame_Per_DIP;
            };

            //プロジェクトに変更が加えられたときには自身のイベントも発火する
            playerViewModel.ProjectChanged += (sender, args) =>
            {
                ProjectChanged?.Invoke(this, EventArgs.Empty);
            };

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(this, layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, layer));
            }

            SelectClip.CollectionChanged += ((sender, args) =>
            {
                foreach (var layerCanvas in LayerCanvas)
                {
                    layerCanvas.ResetSelectedClip();
                }
                playerViewModel.SelectingObjects.Clear();
                foreach (var targetClip in SelectClip)
                {
                    targetClip.IsSelecting = true;
                    playerViewModel.SelectingObjects.Add(targetClip.TargetObject);
                }
            });
        }

        public bool RunEditCommand(IEditCommand command)
        {
            return playerViewModel.RunEditCommand(command);
        }

        public void SetFrameFromPosition(double position)
        {
            Frame = (int)(position / Frame_Per_DIP);
        }

        public void ClipSelect(ClipViewModel clip, bool isMultiSelect = false)
        {
            timelineInteractionService.SelectClip(clip, isMultiSelect);
        }

        public bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame)
        {
            return timelineInteractionService.CanResizeClip(clipObject, newStartFrame, newEndFrame);
        }

        public void ClipsDropped(int moveFrame, int moveLayerCount)
        {
            // This method is now handled by the TimelineInteractionService
            // The behavior is implemented in the MoveClips method of the service
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

        #region ITimelineContext Implementation

        Timeline ITimelineContext.Timeline => TargetTimeline;

        LayerObject ITimelineContext.TargetLayer => TargetLayer;

        void ITimelineContext.RunEditCommand(IEditCommand command)
        {
            RunEditCommand(command);
        }

        #endregion
    }
}
