using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Models.DragDrop;
using Metasia.Editor.Views.Behaviors;

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

        public ICommand HandleDropCommand { get; }
        public ICommand HandleDragOverCommand { get; }
        public ICommand HandleDragLeaveCommand { get; }
        public Interaction<NewObjectSelectViewModel, IMetasiaObject?> NewObjectSelectInteraction { get; } = new();
        public ICommand NewClipCommand { get; }

        private TimelineViewModel parentTimeline;
        public LayerObject TargetLayer { get; private set; }
        public TimelineObject Timeline => parentTimeline.Timeline;

        private readonly IClipViewModelFactory _clipViewModelFactory;
        private readonly IProjectState _projectState;
        private readonly ISelectionState selectionState;
        private readonly IEditCommandManager editCommandManager;
        private readonly ITimelineViewState _timelineViewState;
        private readonly IDropHandlerRegistry _dropHandlerRegistry;
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
            ITimelineViewState timelineViewState,
            IDropHandlerRegistry dropHandlerRegistry)
        {
            this.parentTimeline = parentTimeline;
            TargetLayer = targetLayer;
            _clipViewModelFactory = clipViewModelFactory;
            _projectState = projectState;
            this.selectionState = selectionState;
            this.editCommandManager = editCommandManager;
            this._timelineViewState = timelineViewState;
            this._dropHandlerRegistry = dropHandlerRegistry;

            HandleDropCommand = ReactiveCommand.Create<DropEventData>(
                execute: ExecuteHandleDrop,
                canExecute: this.WhenAnyValue(x => x.TargetLayer).Select(layer => layer != null)
            );
            HandleDragOverCommand = ReactiveCommand.Create<DropEventData>(
                execute: ExecuteHandleDragOver,
                canExecute: this.WhenAnyValue(x => x.TargetLayer).Select(layer => layer != null)
            );
            HandleDragLeaveCommand = ReactiveCommand.Create(ExecuteHandleDragLeave);
            NewClipCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var vm = new NewObjectSelectViewModel(NewObjectSelectViewModel.TargetType.Clip);
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

        public void RecalculateSize()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.RecalculateSize();
            }
        }

        public void EmptyAreaClicked(int frame)
        {
            parentTimeline.SeekFrame(frame);
        }

        private void AddNewClip(ClipObject clipObject)
        {
            int startFrame = parentTimeline.Frame;

            clipObject.StartFrame = startFrame;
            clipObject.EndFrame = startFrame + 100;

            var addCommand = new AddClipCommand(TargetLayer, clipObject);
            editCommandManager.Execute(addCommand);

            RelocateClips();
        }

        private void ExecuteHandleDrop(DropEventData dropEventData)
        {
            editCommandManager.CancelPreview();

            var context = new DropTargetContext(
                dropEventData.TargetLayer,
                dropEventData.TargetFrame,
                dropEventData.Timeline,
                dropEventData.DropPosition
            );

            var handler = _dropHandlerRegistry.FindHandler(dropEventData.Data, context);
            if (handler is null) return;

            var command = handler.HandleDrop(dropEventData.Data, context);
            if (command is not null)
            {
                editCommandManager.Execute(command);
            }
        }

        private void ExecuteHandleDragOver(DropEventData dropEventData)
        {
            editCommandManager.CancelPreview();

            var context = new DropTargetContext(
                dropEventData.TargetLayer,
                dropEventData.TargetFrame,
                dropEventData.Timeline,
                dropEventData.DropPosition
            );

            var handler = _dropHandlerRegistry.FindHandler(dropEventData.Data, context);
            if (handler is null) return;

            var result = handler.HandleDragOver(dropEventData.Data, context);
            if (result?.PreviewCommand is not null)
            {
                editCommandManager.PreviewExecute(result.PreviewCommand);
            }
        }

        private void ExecuteHandleDragLeave()
        {
            editCommandManager.CancelPreview();
        }

        private void RelocateClips()
        {
            List<string> objectIds = TargetLayer.Objects.Select(x => x.Id).ToList();
            List<string> clipIds = ClipsAndBlanks.Select(x => x.TargetObject.Id).ToList();

            var diffIds = objectIds.Except(clipIds).ToList();

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

            var diffIds2 = clipIds.Except(objectIds).ToList();

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

        private void OnCommandExecuted(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandPreviewExecuted(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandUndone(object? sender, IEditCommand e) => RelocateClips();
        private void OnCommandRedone(object? sender, IEditCommand e) => RelocateClips();

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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