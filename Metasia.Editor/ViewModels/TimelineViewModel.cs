using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Xml;
using Metasia.Editor.ViewModels.Timeline;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;

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

        /// <summary>
        /// 水平スクロール位置（フレーム単位）
        /// </summary>
        public int HorizontalScrollPosition
        {
            get => _horizontalScrollPosition;
            set
            {
                this.RaiseAndSetIfChanged(ref _horizontalScrollPosition, value);
                _timelineViewState.HorizontalScrollPosition = value;
            }
        }

        private int _horizontalScrollPosition;

        /// <summary>
        /// プロジェクトのフレームレート
        /// </summary>
        public int FrameRate
        {
            get => _frameRate;
            private set => this.RaiseAndSetIfChanged(ref _frameRate, value);
        }

        /// <summary>
        /// 選択開始位置より前の無効領域の幅（DIP単位）
        /// </summary>
        public double InvalidStartWidth
        {
            get => _invalidStartWidth;
            private set => this.RaiseAndSetIfChanged(ref _invalidStartWidth, value);
        }

        /// <summary>
        /// 選択終了位置より後ろの無効領域の開始位置（DIP単位）
        /// </summary>
        public double InvalidEndLeft
        {
            get => _invalidEndLeft;
            private set => this.RaiseAndSetIfChanged(ref _invalidEndLeft, value);
        }

        /// <summary>
        /// 選択開始位置より前に無効領域が存在するか
        /// </summary>
        public bool HasInvalidStart => Timeline.SelectionStart > 0;

        /// <summary>
        /// 選択終了位置より後ろに無効領域が存在するか
        /// </summary>
        public bool HasInvalidEnd => Timeline.SelectionEnd < TimelineObject.MAX_LENGTH;

        /// <summary>
        /// タイムラインの全体幅（DIP単位）。全レイヤーで共有され、最も長いクリップに合わせる
        /// </summary>
        public double TimelineWidth
        {
            get => _timelineWidth;
            private set => this.RaiseAndSetIfChanged(ref _timelineWidth, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;
        private int _frameRate = 60;

        private int _frame;
        private double _cursorLeft;
        private double _invalidStartWidth;
        private double _invalidEndLeft;
        private double _timelineWidth;

        private readonly ISelectionState selectionState;

        private readonly IPlaybackState playbackState;
        private readonly IEditCommandManager editCommandManager;
        private readonly IProjectState _projectState;
        private readonly ITimelineViewState _timelineViewState;
        private readonly IClipboardService _clipboardService;

        private bool _isUpdatingFramePerDIP = false;

        public TimelineViewModel(
            TimelineObject timeline,
            ILayerButtonViewModelFactory layerButtonViewModelFactory,
            ILayerCanvasViewModelFactory layerCanvasViewModelFactory,
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            ITimelineViewState timelineViewState,
            IClipboardService clipboardService)
        {
            ArgumentNullException.ThrowIfNull(layerButtonViewModelFactory);
            ArgumentNullException.ThrowIfNull(layerCanvasViewModelFactory);
            ArgumentNullException.ThrowIfNull(selectionState);
            ArgumentNullException.ThrowIfNull(projectState);
            ArgumentNullException.ThrowIfNull(editCommandManager);
            ArgumentNullException.ThrowIfNull(timeline);
            ArgumentNullException.ThrowIfNull(clipboardService);
            this.selectionState = selectionState;
            this.playbackState = playbackState;
            _projectState = projectState;
            this.editCommandManager = editCommandManager;
            _timelineViewState = timelineViewState;
            _clipboardService = clipboardService;

            _timeline = timeline;
            _timelineViewState.Frame_Per_DIP_Changed += OnFramePerDIPChanged;
            _timelineViewState.LastPreviewFrame_Changed += OnCurrentFrameChanged;
            _timelineViewState.HorizontalScrollPosition_Changed += OnHorizontalScrollPositionChanged;
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
            _horizontalScrollPosition = _timelineViewState.HorizontalScrollPosition;
            Frame = _timelineViewState.LastPreviewFrame;
            CursorLeft = Frame * _timelineViewState.Frame_Per_DIP;
            if (_projectState.CurrentProjectInfo != null)
            {
                FrameRate = _projectState.CurrentProjectInfo.Framerate;
            }

            playbackState.PlaybackFrameChanged += OnPlaybackFrameChanged;

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(layerButtonViewModelFactory.Create(layer));
                LayerCanvas.Add(layerCanvasViewModelFactory.Create(this, layer));
            }

            editCommandManager.CommandExecuted += OnCommandExecutedForControl;
            editCommandManager.CommandUndone += OnCommandUndoneForControl;
            editCommandManager.CommandRedone += OnCommandRedoneForControl;
            _projectState.TimelineChanged += OnTimelineChangedForWidth;

            UpdateControlLayerHighlights();
            UpdateInvalidAreas();
            UpdateTimelineWidth();
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

                var layer = FindOwnerLayer(obj);
                if (layer is not null)
                {
                    // クリップが存在するレイヤーも選択
                    SelectLayer(layer);
                }
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

            // タイムラインごとの状態として保存
            _timelineViewState.LastPreviewFrame = targetFrame;
        }

        public void ClipRemove(ClipObject clipObject)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                IEditCommand command = new ClipRemoveCommand(clipObject, ownerLayer);
                editCommandManager.Execute(command);
                selectionState.UnselectClip(clipObject);
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

        public void CopySelectedClips()
        {
            var selectedClips = selectionState.SelectedClips.ToList();
            if (selectedClips.Count == 0)
                return;

            var template = ClipTemplateSerializer.CreateFromClips(selectedClips, Timeline);
            var xml = ClipTemplateSerializer.Serialize(template);
            _clipboardService.StoreClips(xml);
        }

        public void CutSelectedClips()
        {
            var selectedClips = selectionState.SelectedClips.ToList();
            if (selectedClips.Count == 0)
                return;

            CopySelectedClips();

            var removeCommands = new List<IEditCommand>();
            foreach (var clip in selectedClips)
            {
                var ownerLayer = FindOwnerLayer(clip);
                if (ownerLayer is not null)
                {
                    removeCommands.Add(new ClipRemoveCommand(clip, ownerLayer));
                }
            }

            if (removeCommands.Count > 0)
            {
                var command = new CompositeCommand(removeCommands, "クリップのカット");
                editCommandManager.Execute(command);
                selectionState.ClearSelectedClips();
            }
        }

        public void PasteClips()
        {
            if (!_clipboardService.HasClips)
                return;

            var xml = _clipboardService.GetStoredClips();
            if (string.IsNullOrEmpty(xml))
                return;

            var template = ClipTemplateSerializer.Deserialize(xml);
            int targetFrame = Frame;

            int startLayerIndex = 0;
            if (selectionState.SelectedLayer != null)
            {
                startLayerIndex = Timeline.Layers.IndexOf(selectionState.SelectedLayer);
                if (startLayerIndex < 0) startLayerIndex = 0;
            }

            var clipsWithLayers = ClipTemplateSerializer.InstantiateClips(template, targetFrame, startLayerIndex, Timeline);

            var validClips = clipsWithLayers
                .Where(item => item.layerIndex >= 0)
                .Select(item => (item.clip, item.layerIndex))
                .ToList();

            if (validClips.Count == 0)
                return;

            var command = new PasteClipsCommand(Timeline, validClips);
            editCommandManager.Execute(command);

            selectionState.ClearSelectedClips();
            foreach (var (clip, _) in command.PlacedClips)
            {
                selectionState.SelectClip(clip);
            }
        }

        public void SelectLayer(LayerObject layer)
        {
            if (selectionState.SelectedLayer?.Id == layer.Id)
                return;
            selectionState.SelectLayer(layer);
        }



        /// <summary>
        /// 指定されたClipObjectに対応するClipViewModelのサイズを再計算する
        /// </summary>
        public void RecalculateClipSize(ClipObject clipObject)
        {
            foreach (var layerCanvas in LayerCanvas)
            {
                var clipVM = layerCanvas.ClipsAndBlanks.FirstOrDefault(c => c.TargetObject.Id == clipObject.Id);
                if (clipVM != null)
                {
                    clipVM.RecalculateSize();
                    return;
                }
            }
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
                _timelineViewState.LastPreviewFrame_Changed -= OnCurrentFrameChanged;
                _timelineViewState.HorizontalScrollPosition_Changed -= OnHorizontalScrollPositionChanged;
                playbackState.PlaybackFrameChanged -= OnPlaybackFrameChanged;
                editCommandManager.CommandExecuted -= OnCommandExecutedForControl;
                editCommandManager.CommandUndone -= OnCommandUndoneForControl;
                editCommandManager.CommandRedone -= OnCommandRedoneForControl;
                _projectState.TimelineChanged -= OnTimelineChangedForWidth;
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

        private void OnCurrentFrameChanged()
        {
            Frame = _timelineViewState.LastPreviewFrame;
            CursorLeft = Frame * _timelineViewState.Frame_Per_DIP;
        }

        private void OnHorizontalScrollPositionChanged()
        {
            _horizontalScrollPosition = _timelineViewState.HorizontalScrollPosition;
            this.RaisePropertyChanged(nameof(HorizontalScrollPosition));
        }

        private void OnPlaybackFrameChanged()
        {
            Frame = playbackState.CurrentFrame;
            CursorLeft = Frame * _timelineViewState.Frame_Per_DIP;
            // 再生中のフレーム位置を状態として保存
            _timelineViewState.LastPreviewFrame = Frame;
        }

        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * _frame_per_DIP;
            UpdateInvalidAreas();
            UpdateTimelineWidth();
        }

        private void UpdateInvalidAreas()
        {
            if (_timeline == null) return;
            InvalidStartWidth = Timeline.SelectionStart * _frame_per_DIP;
            InvalidEndLeft = Timeline.SelectionEnd * _frame_per_DIP;
            this.RaisePropertyChanged(nameof(HasInvalidStart));
            this.RaisePropertyChanged(nameof(HasInvalidEnd));
        }

        private void UpdateTimelineWidth()
        {
            if (_timeline == null) return;
            var maxEndFrame = Timeline.GetLastFrameOfClips();
            double calculatedWidth = maxEndFrame * _frame_per_DIP;
            TimelineWidth = Math.Max(5000, calculatedWidth);
            foreach (var layerCanvas in LayerCanvas)
            {
                layerCanvas.Width = TimelineWidth;
            }
        }

        private void OnTimelineChangedForWidth()
        {
            UpdateControlLayerHighlights();
            UpdateTimelineWidth();
        }

        private void OnCommandExecutedForControl(object? sender, IEditCommand e) { UpdateControlLayerHighlights(); UpdateTimelineWidth(); }
        private void OnCommandUndoneForControl(object? sender, IEditCommand e) { UpdateControlLayerHighlights(); UpdateTimelineWidth(); }
        private void OnCommandRedoneForControl(object? sender, IEditCommand e) { UpdateControlLayerHighlights(); UpdateTimelineWidth(); }

        /// <summary>
        /// 制御系オブジェクト(GroupControl/CameraControl)の影響範囲を計算し、
        /// 対象レイヤーの ControlHighlights / IsUnderControl を更新する
        /// </summary>
        private void UpdateControlLayerHighlights()
        {
            int layerCount = Timeline.Layers.Count;
            var rangesPerLayer = new List<(int StartFrame, int EndFrame)>[layerCount];
            for (int i = 0; i < layerCount; i++)
                rangesPerLayer[i] = new();

            for (int i = 0; i < layerCount; i++)
            {
                var layer = Timeline.Layers[i];
                foreach (var obj in layer.Objects.OfType<ILayerIntervener>())
                {
                    if (obj is ClipObject clip && clip.IsActive)
                    {
                        int targetCount = ((ILayerIntervener)obj).TargetLayers.ToScopeCount();
                        for (int j = 1; j <= targetCount && i + j < layerCount; j++)
                        {
                            rangesPerLayer[i + j].Add((clip.StartFrame, clip.EndFrame));
                        }
                    }
                }
            }

            double framePerDip = _timelineViewState.Frame_Per_DIP;
            for (int i = 0; i < layerCount; i++)
            {
                if (i < LayerCanvas.Count)
                {
                    var canvas = LayerCanvas[i];
                    canvas.ControlHighlights.Clear();
                    foreach (var (startFrame, endFrame) in rangesPerLayer[i])
                    {
                        var info = new Timeline.ControlHighlightInfo
                        {
                            StartFrame = startFrame,
                            EndFrame = endFrame,
                        };
                        info.Recalculate(framePerDip);
                        canvas.ControlHighlights.Add(info);
                    }
                }
            }

            UpdateInvalidAreas();
        }
    }
}
