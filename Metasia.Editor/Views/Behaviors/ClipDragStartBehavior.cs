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
    /// タイムラインクリップのドラッグ開始ビヘイビア
    /// </summary>
    public class ClipDragStartBehavior : Behavior<Control>
    {
        public static readonly StyledProperty<ICommand?> CommandProperty =
            AvaloniaProperty.Register<ClipDragStartBehavior, ICommand?>(nameof(Command));
        
        public static readonly StyledProperty<double> DragThresholdProperty =
            AvaloniaProperty.Register<ClipDragStartBehavior, double>(nameof(DragThreshold), 5.0);
        
        public static readonly StyledProperty<double> FramePerDIPProperty =
            AvaloniaProperty.Register<ClipDragStartBehavior, double>(nameof(FramePerDIP), 1.0);
        
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
        private PointerEventArgs? _lastPointerEventArgs;
        
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is not null)
            {
                AssociatedObject.PointerPressed += OnPointerPressed;
                AssociatedObject.PointerMoved += OnPointerMoved;
                AssociatedObject.PointerReleased += OnPointerReleased;
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
            _lastPointerEventArgs = e;
            
            if (_isDraggingPotential && e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed && _startPoint.HasValue)
            {
                var currentPoint = e.GetCurrentPoint(AssociatedObject).Position;
                if (Math.Abs(currentPoint.X - _startPoint.Value.X) > DragThreshold)
                {
                    _isDraggingPotential = false;
                    
                    // ViewModelを取得
                    var vm = AssociatedObject?.DataContext as ClipViewModel;
                    if (vm is not null)
                    {
                        var dragData = new DataObject();
                        const string dragFormat = "ClipsMoveDragData";
                        dragData.Set(dragFormat, new ClipsMoveDragData(vm, CalculateTargetFrame(_startPoint.Value.X)));
                        
                        // 実際のドラッグ&ドロップを開始
                        await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
                    }
                }
            }
        }
        
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDraggingPotential = false;
            _startPoint = null;
            _lastPointerEventArgs = null;
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