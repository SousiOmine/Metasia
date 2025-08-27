using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Controls;
using Metasia.Editor.Views.Controls;

namespace Metasia.Editor.Views.Behaviors
{
    /// <summary>
    /// タイムラインクリップのドラッグ開始ビヘイビア
    /// </summary>
    public class ClipViewBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand?> CommandProperty =
            AvaloniaProperty.Register<ClipViewBehavior, ICommand?>(nameof(Command));
        
        public static readonly StyledProperty<double> DragThresholdProperty =
            AvaloniaProperty.Register<ClipViewBehavior, double>(nameof(DragThreshold), 5.0);
        
        public static readonly StyledProperty<double> FramePerDIPProperty =
            AvaloniaProperty.Register<ClipViewBehavior, double>(nameof(FramePerDIP), 1.0);
        
        public ICommand? Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        
        public double DragThreshold
        {
            get => GetValue(DragThresholdProperty);
            set => SetValue(DragThresholdProperty, value);
        }

        public double FramePerDIP
        {
            get => GetValue(FramePerDIPProperty);
            set => SetValue(FramePerDIPProperty, value);
        }
        
        private Point? _startPoint;
        private bool _isDraggingPotential;
        private string _dragHandleName = string.Empty;
        private bool _isHandleDragging = false;
        
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is not null)
            {
                AssociatedObject.PointerPressed += OnPointerPressed;
                AssociatedObject.PointerMoved += OnPointerMoved;
                AssociatedObject.PointerReleased += OnPointerReleased;
                
                // ハンドルのイベントを登録
                AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;
            }
        }
        
        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject is not null)
            {
                AssociatedObject.PointerPressed -= OnPointerPressed;
                AssociatedObject.PointerMoved -= OnPointerMoved;
                AssociatedObject.PointerReleased -= OnPointerReleased;
                AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
            }
        }
        
        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // ハンドル要素を取得してイベントを登録
            if (AssociatedObject is Control control)
            {
                var startHandle = control.FindControl<Border>("StartHandle");
                var endHandle = control.FindControl<Border>("EndHandle");
                
                if (startHandle != null)
                {
                    startHandle.PointerPressed += Handle_PointerPressed;
                    startHandle.PointerMoved += Handle_PointerMoved;
                    startHandle.PointerReleased += Handle_PointerReleased;
                }
                
                if (endHandle != null)
                {
                    endHandle.PointerPressed += Handle_PointerPressed;
                    endHandle.PointerMoved += Handle_PointerMoved;
                    endHandle.PointerReleased += Handle_PointerReleased;
                }
            }
        }
        
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _startPoint = e.GetCurrentPoint(AssociatedObject).Position;
                _isDraggingPotential = true;
            }
        }
        
        private async void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(AssociatedObject).Position;
            
            if (_isDraggingPotential && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed && _startPoint.HasValue && Math.Abs(currentPoint.X - _startPoint.Value.X) > DragThreshold)
            {
                _isDraggingPotential = false;
                await StartDragDropAsync(e);
            }
        }
        
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDraggingPotential = false;
            _startPoint = null;
        }

        /// <summary>
        /// ハンドルのポインター押下処理
        /// </summary>
        private void Handle_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // ViewModelがnullか、Borderからのイベントでなければ何もしない
            if (sender is not Border handle) return;
            if (handle.Name != "StartHandle" && handle.Name != "EndHandle")
            {
                return;
            }

            var vm = AssociatedObject?.DataContext as ClipViewModel;
            if (vm is null) return;

            var parentCanvas = AssociatedObject?.Parent as Control;
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;

            // ViewModelでドラッグ開始処理
            vm.StartDrag(handle.Name, position.X);

            e.Pointer.Capture(handle);
            e.Handled = true;
            
            _isHandleDragging = true;
            _dragHandleName = handle.Name;
        }

        /// <summary>
        /// ハンドルのポインター移動処理
        /// </summary>
        private void Handle_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isHandleDragging || sender is not Border handle) return;
            if (e.Pointer.Captured == handle)
            {
                var vm = AssociatedObject?.DataContext as ClipViewModel;
                if (vm is null) return;

                var parentCanvas = AssociatedObject?.Parent as Control;
                if (parentCanvas is null) return;

                var position = e.GetCurrentPoint(parentCanvas).Position;

                vm.UpdateDrag(position.X);
                e.Handled = true;
            }
        }

        /// <summary>
        /// ハンドルのポインター解放処理
        /// </summary>
        private void Handle_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isHandleDragging || sender is not Border handle) return;
            if (e.Pointer.Captured == handle)
            {
                var vm = AssociatedObject?.DataContext as ClipViewModel;
                if (vm is null) return;

                var parentCanvas = AssociatedObject?.Parent as Control;
                if (parentCanvas is null) return;

                var position = e.GetCurrentPoint(parentCanvas).Position;
                vm.EndDrag(position.X);

                e.Handled = true;
            }
            e.Pointer.Capture(null);
            _isHandleDragging = false;
            _dragHandleName = string.Empty;
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

        private async Task StartDragDropAsync(PointerEventArgs e)
        {
            // ViewModelを取得
            var vm = AssociatedObject?.DataContext as ClipViewModel;
            if (vm is not null && _startPoint.HasValue)
            {
                // ClipViewにドラッグ開始を通知
                if (AssociatedObject is ClipView clipView)
                {
                    clipView.NotifyDragStarted();
                }
                
                var dragData = new DataObject();
                dragData.Set(DragDropFormats.ClipsMove, new ClipsMoveDragData(vm, CalculateTargetFrame(_startPoint.Value.X)));
                        
                // 実際のドラッグ&ドロップを開始
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            }
        }
    }
}
