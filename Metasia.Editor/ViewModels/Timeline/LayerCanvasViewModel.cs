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
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.ViewModels.Timeline
{
    public class LayerCanvasViewModel : ViewModelBase
    {
        public ObservableCollection<ClipViewModel> ClipsAndBlanks { get; set; } = new();

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            private set => this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
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
        public ICommand HandleDragOverCommand { get; }
        public ICommand HandleDragLeaveCommand { get; }
        public Interaction<NewObjectSelectViewModel, IMetasiaObject?> NewObjectSelectInteraction { get; } = new();
        public ICommand NewClipCommand { get; }

        private TimelineViewModel parentTimeline;
        public LayerObject TargetLayer { get; private set; }

        private readonly IClipViewModelFactory _clipViewModelFactory;
        private readonly IProjectState _projectState;
        private readonly ISelectionState selectionState;
        private readonly IEditCommandManager editCommandManager;
        private readonly ITimelineViewState _timelineViewState;
        private double _frame_per_DIP;
        private double width;
        private bool _disposed;

        public LayerCanvasViewModel(
            TimelineViewModel parentTimeline,
            LayerObject targetLayer,
            IClipViewModelFactory clipViewModelFactory,
            IProjectState projectState,
            ISelectionState selectionState,
            IEditCommandManager editCommandManager,
            ITimelineViewState timelineViewState)
        {
            this.parentTimeline = parentTimeline;
            TargetLayer = targetLayer;
            _clipViewModelFactory = clipViewModelFactory;
            _projectState = projectState;
            this.selectionState = selectionState;
            this.editCommandManager = editCommandManager;
            this._timelineViewState = timelineViewState;
            // ドロップ処理コマンドの初期化
            HandleDropCommand = ReactiveCommand.Create<ClipsDropTargetContext>(
                execute: ExecuteHandleDrop,
                canExecute: this.WhenAnyValue(x => x.TargetLayer).Select(layer => layer != null)
            );
            HandleDragOverCommand = ReactiveCommand.Create<ClipsDropTargetContext>(
                execute: ExecuteHandleDragOver,
                canExecute: this.WhenAnyValue(x => x.TargetLayer).Select(layer => layer != null)
            );
            HandleDragLeaveCommand = ReactiveCommand.Create(ExecuteHandleDragLeave);
            NewClipCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var vm = new NewObjectSelectViewModel();
                var result = await NewObjectSelectInteraction.Handle(vm);

                if (result is not null && result is ClipObject clipObject)
                {
                    AddNewClip(clipObject);
                }
            });

            _timelineViewState.Frame_Per_DIP_Changed += OnFramePerDIPChangedWithRecalculation;
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;

            RelocateClips();

            _projectState.ProjectLoaded += OnProjectLoaded;
            _projectState.TimelineChanged += OnTimelineChanged;

            selectionState.SelectionChanged += OnSelectionChangedWithClipSelection;

            // コマンド実行時にUIを更新する
            editCommandManager.CommandExecuted += OnCommandExecuted;
            editCommandManager.CommandPreviewExecuted += OnCommandPreviewExecuted;
            editCommandManager.CommandUndone += OnCommandUndone;
            editCommandManager.CommandRedone += OnCommandRedone;
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
        /// 空のエリアがクリックされたときに呼び出されるメソッド
        /// </summary>
        /// <param name="frame">クリックされた位置のフレーム</param>
        public void EmptyAreaClicked(int frame)
        {
            // フレーム位置にプレビューを移動
            parentTimeline.SeekFrame(frame);
        }

        private void AddNewClip(ClipObject clipObject)
        {
            // 新しいクリップを適切な位置に追加（現在のカーソル位置か、レイヤーの最後）
            int startFrame = parentTimeline.Frame;

            // クリップの基本プロパティを設定
            clipObject.StartFrame = startFrame;
            clipObject.EndFrame = startFrame + 100; // デフォルトで100フレーム

            // コマンドマネージャーに追加操作を記録
            var addCommand = new AddClipCommand(TargetLayer, clipObject);
            editCommandManager.Execute(addCommand);

            // UIを更新
            RelocateClips();
        }

        /// <summary>
        /// タイムラインVMのドロップ処理を呼び出す
        /// </summary>
        private void ExecuteHandleDrop(ClipsDropTargetContext dropInfo)
        {
            editCommandManager.CancelPreview();

            // スナッピングを適用
            ClipInteractor.ApplyMoveSnapping(
                dropInfo,
                selectionState.SelectedClips,
                parentTimeline.Timeline,
                _timelineViewState.Frame_Per_DIP);

            var command = ClipInteractor.CreateMoveClipsCommand(
                dropInfo,
                parentTimeline.Timeline,
                TargetLayer,
                selectionState.SelectedClips);

            if (command is not null)
            {
                editCommandManager.Execute(command);
            }
        }

        private void ExecuteHandleDragOver(ClipsDropTargetContext dropInfo)
        {
            // 既存のプレビューがあればキャンセルして元の状態に戻す
            editCommandManager.CancelPreview();

            // スナッピングを適用
            ClipInteractor.ApplyMoveSnapping(
                dropInfo,
                selectionState.SelectedClips,
                parentTimeline.Timeline,
                _timelineViewState.Frame_Per_DIP);

            var command = ClipInteractor.CreateMoveClipsCommand(
                dropInfo,
                parentTimeline.Timeline,
                TargetLayer,
                selectionState.SelectedClips);

            if (command is not null)
            {
                editCommandManager.PreviewExecute(command);
            }
        }

        private void ExecuteHandleDragLeave()
        {
            editCommandManager.CancelPreview();
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
                    var clipVM = _clipViewModelFactory.Create(obj, parentTimeline);
                    ClipsAndBlanks.Add(clipVM);
                    if (selectionState.SelectedClips.Any(x => x.Id == id))
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
            var maxEndFrame = TargetLayer.Objects.Count > 0 ? TargetLayer.Objects.Max(o => o.EndFrame) : 0;
            double calculatedWidth = maxEndFrame * _timelineViewState.Frame_Per_DIP;
            Width = Math.Max(5000, calculatedWidth);
        }

        // Event handler methods for proper cleanup
        private void OnCommandExecuted(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandPreviewExecuted(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandUndone(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandRedone(object? sender, IEditCommand e) => RelocateClips();

        /// <summary>
        /// リソースの解放処理
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // イベントハンドラーの購読を解除
                    if (editCommandManager != null)
                    {
                        editCommandManager.CommandExecuted -= OnCommandExecuted;
                        editCommandManager.CommandPreviewExecuted -= OnCommandPreviewExecuted;
                        editCommandManager.CommandUndone -= OnCommandUndone;
                        editCommandManager.CommandRedone -= OnCommandRedone;
                    }

                    if (_timelineViewState != null)
                    {
                        _timelineViewState.Frame_Per_DIP_Changed -= OnFramePerDIPChangedWithRecalculation;
                    }

                    if (_projectState != null)
                    {
                        _projectState.ProjectLoaded -= OnProjectLoaded;
                        _projectState.TimelineChanged -= OnTimelineChanged;
                    }

                    if (selectionState != null)
                    {
                        selectionState.SelectionChanged -= OnSelectionChangedWithClipSelection;
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        // Event handler methods for proper cleanup
        private void OnFramePerDIPChangedWithRecalculation()
        {
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
            ChangeFramePerDIP();
        }

        private void OnProjectLoaded() => RelocateClips();
        private void OnTimelineChanged() => RelocateClips();
        private void OnSelectionChangedWithClipSelection()
        {
            ResetSelectedClip();
            foreach (var obj in selectionState.SelectedClips)
            {
                var clip = ClipsAndBlanks.FirstOrDefault(c => c.TargetObject.Id == obj.Id);
                if (clip is not null)
                {
                    clip.IsSelecting = true;
                }
            }
        }
        private void OnSelectionChanged() => ResetSelectedClip();
    }
}
