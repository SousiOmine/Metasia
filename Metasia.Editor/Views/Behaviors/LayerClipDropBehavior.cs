using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Timeline;

namespace Metasia.Editor.Views.Behaviors
{
    /// <summary>
    /// タイムラインレイヤーへのクリップドロップ処理ビヘイビア
    /// </summary>
    public class LayerClipDropBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand?> DropCommandProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, ICommand?>(nameof(DropCommand));

        public static readonly StyledProperty<ICommand?> DragOverCommandProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, ICommand?>(nameof(DragOverCommand));

        public static readonly StyledProperty<ICommand?> DragLeaveCommandProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, ICommand?>(nameof(DragLeaveCommand));

        public static readonly StyledProperty<double> FramePerDIPProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, double>(nameof(FramePerDIP), 1.0);

        public ICommand? DropCommand
        {
            get => GetValue(DropCommandProperty);
            set => SetValue(DropCommandProperty, value);
        }

        public ICommand? DragOverCommand
        {
            get => GetValue(DragOverCommandProperty);
            set => SetValue(DragOverCommandProperty, value);
        }

        public ICommand? DragLeaveCommand
        {
            get => GetValue(DragLeaveCommandProperty);
            set => SetValue(DragLeaveCommandProperty, value);
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
                AssociatedObject.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
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
                AssociatedObject.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
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
                if (DragOverCommand?.CanExecute(dropInfo) == true)
                {
                    DragOverCommand.Execute(dropInfo);
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void OnDragLeave(object? sender, RoutedEventArgs e)
        {
            if (DragLeaveCommand?.CanExecute(null) == true)
            {
                DragLeaveCommand.Execute(null);
            }
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

        private ClipsDropTargetContext? CreateDropTargetInfo(DragEventArgs e)
        {
            if (e.Data.Get(DragDropFormats.ClipsMove) is ClipsMoveDragData clipsMoveDragData && AssociatedObject is not null)
            {
                var position = e.GetPosition(AssociatedObject);
                return new ClipsDropTargetContext(clipsMoveDragData, CalculateTargetFrame(position.X), true);
            }
            return null;
        }

        /// <summary>
        /// マウス座標からフレーム(クリップ始点が0)に変換
        /// </summary>
        /// <param name="positionX"></param>
        /// <returns></returns>
        private int CalculateTargetFrame(double positionX)
        {
            return (int)(positionX / FramePerDIP);
        }


    }
}