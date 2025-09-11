using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Timeline;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels
{
    [Serializable]
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
                if (_isUpdatingFramePerDIP) return;

                this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
                _timelineViewState.Frame_Per_DIP = value;
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
        /// 現在表示しているフレーム
        /// </summary>
        public int Frame
        {
            get => _frame;
            private set => this.RaiseAndSetIfChanged(ref _frame, value);
        }

        /// <summary>
        /// タイムラインのカーソルの位置
        /// </summary>
        public double CursorLeft
        {
            get => _cursorLeft;
            private set => this.RaiseAndSetIfChanged(ref _cursorLeft, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;

        private int _frame;
        private double _cursorLeft;

        private readonly ISelectionState selectionState;

        private readonly IPlaybackState playbackState;
        private readonly IEditCommandManager editCommandManager;
        private readonly IProjectState _projectState;
        private readonly ITimelineViewState _timelineViewState;

        private bool _isUpdatingFramePerDIP = false;

        public TimelineViewModel(
            ILayerButtonViewModelFactory layerButtonViewModelFactory,
            ILayerCanvasViewModelFactory layerCanvasViewModelFactory,
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            ITimelineViewState timelineViewState)
        {
            ArgumentNullException.ThrowIfNull(layerButtonViewModelFactory);
            ArgumentNullException.ThrowIfNull(layerCanvasViewModelFactory);
            ArgumentNullException.ThrowIfNull(selectionState);
            ArgumentNullException.ThrowIfNull(projectState);
            ArgumentNullException.ThrowIfNull(editCommandManager);
            ArgumentNullException.ThrowIfNull(projectState.CurrentTimeline);
            this.selectionState = selectionState;
            this.playbackState = playbackState;
            _projectState = projectState;
            this.editCommandManager = editCommandManager;
            _timelineViewState = timelineViewState;

            _timelineViewState.Frame_Per_DIP_Changed += OnFramePerDIPChanged;
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
            _timeline = _projectState.CurrentTimeline;

            playbackState.PlaybackFrameChanged += OnPlaybackFrameChanged;

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(layerButtonViewModelFactory.Create(layer));
                LayerCanvas.Add(layerCanvasViewModelFactory.Create(this, layer));
            }
        }


        public void ClipSelect(ClipObject obj, bool isMultiSelect = false)
        {
            if (isMultiSelect)
            {
                // 複数選択モード：既に選択されている場合は選択解除、そうでなければ追加
                if (selectionState.SelectedClips.Contains(obj))
                {
                    selectionState.UnselectClip(obj);
                }
                else
                {
                    selectionState.SelectClip(obj);
                }
            }
            else
            {
                // 単一選択モード：既存の選択をクリアして新しいクリップを選択
                selectionState.ClearSelectedClips();
                selectionState.SelectClip(obj);
            }
        }

        public void SeekFrame(int targetFrame)
        {
            // 再生中なら停止
            if (playbackState.IsPlaying)
            {
                playbackState.Pause();
            }
            
            // プレビュー位置を移動
            playbackState.Seek(targetFrame);
        }

        public void ClipRemove(ClipObject clipObject)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                IEditCommand command = new ClipRemoveCommand(clipObject, ownerLayer);
                editCommandManager.Execute(command);
            }
        }

        public void SplitSelectedClips()
        {
            // 現在のフレーム位置で選択中のクリップを分割
            int splitFrame = Frame;
            
            // 選択中のクリップをフィルタリング（分割可能なもののみ）
            var selectedClips = selectionState.SelectedClips
                .Where(clip => clip is ClipObject clipObject && 
                              splitFrame > clipObject.StartFrame && 
                              splitFrame < clipObject.EndFrame)
                .Cast<ClipObject>()
                .ToList();
            
            if (selectedClips.Count == 0)
                return;
            
            // 各クリップのオーナーレイヤーを取得
            var ownerLayers = selectedClips.Select(clip => FindOwnerLayer(clip)).ToList();
            
            // すべてのクリップにオーナーレイヤーがあるか確認
            if (ownerLayers.Any(layer => layer is null))
                return;

            IEditCommand command = new ClipsSplitCommand(selectedClips, ownerLayers!, splitFrame);
            editCommandManager.Execute(command);
        }

        public bool CanResizeClip(ClipObject clipObject, int newStartFrame, int newEndFrame)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                return ownerLayer.CanPlaceObjectAt(clipObject, newStartFrame, newEndFrame);
            }
            return false;
        }

        
        /// <summary>
        /// リソースの解放処理
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // イベントハンドラーの購読解除
                _timelineViewState.Frame_Per_DIP_Changed -= OnFramePerDIPChanged;
                playbackState.PlaybackFrameChanged -= OnPlaybackFrameChanged;
            }

            base.Dispose(disposing);
        }

        private LayerObject? FindOwnerLayer(ClipObject targetObject)
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



        private void OnFramePerDIPChanged()
        {
            _isUpdatingFramePerDIP = true;
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
            _isUpdatingFramePerDIP = false;
        }

        private void OnPlaybackFrameChanged()
        {
            Frame = playbackState.CurrentFrame;
            CursorLeft = Frame * _timelineViewState.Frame_Per_DIP;
        }

        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * _frame_per_DIP;
        }
    }
}
