using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Views.Timeline;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;

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

        // マウス操作の状態管理
        private DateTime _mousePressStartTime;
        private bool _hasStartedDrag = false;
        private const int CLICK_THRESHOLD_MS = 300;

        private Point? _mousePressStartPoint;
        private bool _isDragReady = false;
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
            e.Handled = true;

            // マウスボタンが押された時間を記録
            _mousePressStartTime = DateTime.Now;
            _hasStartedDrag = false;

            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                // 左クリックの場合、ドラッグの準備を開始
                _mousePressStartPoint = e.GetCurrentPoint(AssociatedObject).Position;
                _isDragReady = true;
            }
            else
            {
                // 右クリックの場合、即座に選択処理を実行
                var properties = e.GetCurrentPoint(AssociatedObject).Properties;
                if (properties.IsRightButtonPressed)
                {
                    TryClipSelect(e.KeyModifiers, e);
                }
            }
        }

        private async void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(AssociatedObject).Position;

            // ドラッグの準備ができていて、左ボタンが押されていて、移動距離が閾値を超えている場合
            if (_isDragReady && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed && _mousePressStartPoint.HasValue && Math.Abs(currentPoint.X - _mousePressStartPoint.Value.X) > DragThreshold)
            {
                _isDragReady = false;
                _hasStartedDrag = true; // ドラッグ開始を通知
                e.Handled = true;
                await StartDragDropAsync(e);
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Handled = true;

            // マウスボタンが押されていた時間と現在時刻の差を計算
            var pressDuration = (DateTime.Now - _mousePressStartTime).TotalMilliseconds;

            // 短時間クリックかつドラッグが開始されていない場合のみ選択処理
            if (pressDuration < CLICK_THRESHOLD_MS && !_hasStartedDrag)
            {
                TryClipSelect(e.KeyModifiers, e);
            }

            // 状態をリセット
            _isDragReady = false;
            _hasStartedDrag = false;
            _mousePressStartPoint = null;
        }

        private void TryClipSelect(KeyModifiers modifiers, PointerEventArgs? pointerEventArgs)
        {
            var vm = AssociatedObject?.DataContext as ClipViewModel;
            if (vm is null) return;

            // キーバインディングサービスから修飾キー設定を取得
            var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            var multiSelectModifier = keyBindingService?.GetModifierForAction("MultiSelectClip");

            bool isMultiSelect = multiSelectModifier.HasValue &&
                                keyBindingService != null && keyBindingService.IsModifierKeyPressed(multiSelectModifier.Value, modifiers);

            // クリックされた位置からフレームを計算してViewModelに通知
            if (pointerEventArgs != null && AssociatedObject != null)
            {
                pointerEventArgs.Handled = true;

                var position = pointerEventArgs.GetCurrentPoint(AssociatedObject).Position;
                // クリップ内の相対位置を計算
                var relativePositionX = position.X;
                // 相対位置をフレームに変換
                var frameOffset = (int)(relativePositionX / FramePerDIP);
                // クリップの開始フレームにオフセットを加算して絶対フレーム位置を算出
                var targetFrame = vm.TargetObject.StartFrame + frameOffset;

                vm.ClipClick(isMultiSelect, targetFrame);
            }
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

            var parentCanvas = GetParentCanvas();
            if (parentCanvas is null) return;

            var position = e.GetCurrentPoint(parentCanvas).Position;

            // ViewModelでドラッグ開始処理
            vm.StartDrag(handle.Name, position.X);

            e.Pointer.Capture(handle);
            e.Handled = true;

            _isHandleDragging = true;
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

                var parentCanvas = GetParentCanvas();
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

                var parentCanvas = GetParentCanvas();
                if (parentCanvas is null) return;

                var position = e.GetCurrentPoint(parentCanvas).Position;
                vm.EndDrag(position.X);

                e.Handled = true;
            }
            e.Pointer.Capture(null);
            _isHandleDragging = false;
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
            if (vm is not null && _mousePressStartPoint.HasValue)
            {
                // ClipViewにドラッグ開始を通知
                if (AssociatedObject is ClipView clipView)
                {
                    clipView.NotifyDragStarted();
                }

                var dragData = new DataObject();
                dragData.Set(DragDropFormats.ClipsMove, new ClipsMoveDragData(vm, CalculateTargetFrame(_mousePressStartPoint.Value.X)));

                // 実際のドラッグ&ドロップを開始
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            }
        }

        private Control? GetParentCanvas()
        {
            var current = AssociatedObject?.Parent as Control;
            while (current != null)
            {
                if (current is ItemsControl)
                {
                    return current;
                }
                current = current.Parent as Control;
            }
            return null;
        }
    }
}
