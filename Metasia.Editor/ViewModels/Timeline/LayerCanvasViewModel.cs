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
            NewClipCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                var vm = new NewObjectSelectViewModel();
                var result = await NewObjectSelectInteraction.Handle(vm);

                if (result is not null)
                {
                    //TODO: クリップ新規追加処理
                    Console.WriteLine("New Clip Selected");
                }
            });

            _timelineViewState.Frame_Per_DIP_Changed += () =>
            {
                Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
                ChangeFramePerDIP();
            };
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;

            RelocateClips();

            _projectState.ProjectLoaded += () =>
            {
                RelocateClips();
            };
            _projectState.TimelineChanged += () =>
            {
                RelocateClips();
            };

            selectionState.SelectionChanged += () =>
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
        /// 空のエリアがクリックされたときに呼び出されるメソッド
        /// </summary>
        /// <param name="frame">クリックされた位置のフレーム</param>
        public void EmptyAreaClicked(int frame)
        {
            // フレーム位置にプレビューを移動
            parentTimeline.SeekFrame(frame);
        }

        /// <summary>
        /// タイムラインVMのドロップ処理を呼び出す
        /// </summary>
        private void ExecuteHandleDrop(ClipsDropTargetContext dropInfo)
        {
            var command = TimelineInteractor.CreateMoveClipsCommand(dropInfo, parentTimeline.Timeline, TargetLayer, selectionState.SelectedClips);
            if (command is not null)
            {
                editCommandManager.Execute(command);
            }
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
            Width = TargetLayer.EndFrame * _timelineViewState.Frame_Per_DIP;
        }
    }
}
