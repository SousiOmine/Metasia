using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.Views.Behaviors
{
    /// <summary>
    /// タイムラインレイヤーへのクリップドロップ処理ビヘイビア
    /// </summary>
    public class LayerClipDropBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand?> DropCommandProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, ICommand?>(nameof(DropCommand));
        
        public static readonly StyledProperty<double> FramePerDIPProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, double>(nameof(FramePerDIP), 1.0);
        
        public ICommand? DropCommand
        {
            get => GetValue(DropCommandProperty);
            set => SetValue(DropCommandProperty, value);
        }
        
        public double FramePerDIP
        {
            get => GetValue(FramePerDIPProperty);
            set => SetValue(FramePerDIPProperty, value);
        }
        
        /// <summary>
        /// Attaches drag-and-drop event handlers to the associated control and enables drop functionality.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is not null)
            {
                DragDrop.SetAllowDrop(AssociatedObject, true);
                AssociatedObject.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AssociatedObject.AddHandler(DragDrop.DragOverEvent, OnDragOver);
                AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop);
            }
        }
        
        /// <summary>
        /// Detaches the drag-and-drop event handlers from the associated control when the behavior is removed.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject is not null)
            {
                AssociatedObject.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
                AssociatedObject.RemoveHandler(DragDrop.DropEvent, OnDrop);
            }
        }
        
        /// <summary>
        /// Handles the drag enter event to determine if the dragged data can be dropped onto the control, setting the appropriate drag effect.
        /// </summary>
        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            var dropInfo = CreateDropTargetInfo(e);
            if (dropInfo is not null && DropCommand?.CanExecute(dropInfo) == true)
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        
        /// <summary>
        /// Handles the drag-over event to determine if the dragged data can be dropped onto the control, updating the drag effect accordingly.
        /// </summary>
        private void OnDragOver(object? sender, DragEventArgs e)
        {
            var dropInfo = CreateDropTargetInfo(e);
            if (dropInfo is not null && DropCommand?.CanExecute(dropInfo) == true)
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        
        /// <summary>
        /// Handles the drop event by creating drop target information from the drag event and executing the drop command if applicable.
        /// </summary>
        private void OnDrop(object? sender, DragEventArgs e)
        {
            var dropInfo = CreateDropTargetInfo(e);
            if (dropInfo is not null)
            {
                DropCommand?.Execute(dropInfo);
            }
            e.Handled = true;
        }
        
        /// <summary>
        /// Creates a <see cref="ClipsDropTargetInfo"/> object from the drag event if valid clip move data is present and the associated control exists.
        /// </summary>
        /// <param name="e">The drag event arguments containing the drag data and drop position.</param>
        /// <returns>
        /// A <see cref="ClipsDropTargetInfo"/> with the extracted drag data and drop position if the data is valid; otherwise, <c>null</c>.
        /// </returns>
        private ClipsDropTargetInfo? CreateDropTargetInfo(DragEventArgs e)
        {
            if (e.Data.Get("ClipsMoveDragData") is ClipsMoveDragData clipsMoveDragData && AssociatedObject is not null)
            {
                var position = e.GetPosition(AssociatedObject);
                return new ClipsDropTargetInfo
                {
                    DragData = clipsMoveDragData,
                    DropPositionX = position.X,
                    CanDrop = true
                };
            }
            return null;
        }
    }
} 