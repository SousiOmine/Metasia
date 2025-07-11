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
        
        private Point? _startPoint;
        private bool _isDraggingPotential;
        private PointerEventArgs? _lastPointerEventArgs;
        
        /// <summary>
        /// Subscribes to pointer events on the associated control when the behavior is attached.
        /// </summary>
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
        
        /// <summary>
        /// Unsubscribes from pointer events when the behavior is detached from its associated control.
        /// </summary>
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
        
        /// <summary>
        /// Handles the pointer pressed event to record the initial position when the left mouse button is pressed, enabling potential drag initiation.
        /// </summary>
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            {
                _startPoint = e.GetCurrentPoint(AssociatedObject).Position;
                _isDraggingPotential = true;
            }
        }
        
        /// <summary>
        /// Initiates a drag-and-drop operation for a timeline clip when the pointer moves horizontally beyond the drag threshold while the left mouse button is pressed.
        /// </summary>
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
                        dragData.Set(dragFormat, new ClipsMoveDragData(vm, _startPoint.Value.X, vm.Frame_Per_DIP));
                        
                        // 実際のドラッグ&ドロップを開始
                        await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
                    }
                }
            }
        }
        
        /// <summary>
        /// Resets the drag initiation state when the pointer is released.
        /// </summary>
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDraggingPotential = false;
            _startPoint = null;
            _lastPointerEventArgs = null;
        }
    }
} 