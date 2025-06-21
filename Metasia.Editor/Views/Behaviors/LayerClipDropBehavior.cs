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
        
        private void OnDrop(object? sender, DragEventArgs e)
        {
            var dropInfo = CreateDropTargetInfo(e);
            if (dropInfo is not null)
            {
                DropCommand?.Execute(dropInfo);
            }
            e.Handled = true;
        }
        
        private DropTargetInfo? CreateDropTargetInfo(DragEventArgs e)
        {
            if (e.Data.Get("ClipMoveDragData") is ClipMoveDragData clipMoveDragData && AssociatedObject is not null)
            {
                var position = e.GetPosition(AssociatedObject);
                return new DropTargetInfo
                {
                    DragData = clipMoveDragData,
                    DropPositionX = position.X,
                    CanDrop = true
                };
            }
            return null;
        }
    }
} 