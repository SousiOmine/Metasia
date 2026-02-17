using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Metasia.Core.Objects;

namespace Metasia.Editor.Views.Behaviors
{
    /// <summary>
    /// タイムラインレイヤーへのドロップ処理ビヘイビア
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

        public static readonly StyledProperty<LayerObject?> TargetLayerProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, LayerObject?>(nameof(TargetLayer));

        public static readonly StyledProperty<TimelineObject?> TimelineProperty =
            AvaloniaProperty.Register<LayerClipDropBehavior, TimelineObject?>(nameof(Timeline));

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

        public LayerObject? TargetLayer
        {
            get => GetValue(TargetLayerProperty);
            set => SetValue(TargetLayerProperty, value);
        }

        public TimelineObject? Timeline
        {
            get => GetValue(TimelineProperty);
            set => SetValue(TimelineProperty, value);
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
            var context = CreateDropContext(e);
            if (context != null && DropCommand?.CanExecute(context) == true)
            {
                e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            var context = CreateDropContext(e);
            if (context != null && DragOverCommand?.CanExecute(context) == true)
            {
                DragOverCommand.Execute(context);
                e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
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
            var context = CreateDropContext(e);
            if (context != null)
            {
                DropCommand?.Execute(context);
            }
            e.Handled = true;
        }

        private DropEventData? CreateDropContext(DragEventArgs e)
        {
            if (AssociatedObject == null || TargetLayer == null || Timeline == null)
                return null;

            var position = e.GetPosition(AssociatedObject);
            return new DropEventData(
                e.Data,
                TargetLayer,
                CalculateTargetFrame(position.X),
                Timeline,
                position
            );
        }

        private int CalculateTargetFrame(double positionX)
        {
            return (int)(positionX / FramePerDIP);
        }
    }

    /// <summary>
    /// ドロップイベントのデータ
    /// </summary>
    public record DropEventData(
        IDataObject Data,
        LayerObject TargetLayer,
        int TargetFrame,
        TimelineObject Timeline,
        Point DropPosition
    );
}