using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Abstractions.States;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Views;

public partial class TimelineView : UserControl
{
    private TimelineViewModel? VM
    {
        get { return this.DataContext as TimelineViewModel; }
    }

    private readonly IKeyBindingService? _keyBindingService;
    private KeyModifiers _timelineZoomModifier = KeyModifiers.Control;
    private bool _isDraggingTimeline = false;
    private TimelineViewModel? _subscribedVM;
    private bool _isUpdatingScrollFromViewModel = false;
    private bool _isUpdatingScrollFromView = false;

    public TimelineView()
    {
        InitializeComponent();

        // キーバインディングサービスを取得
        _keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        LoadTimelineZoomModifier();

        DataContextChanged += OnDataContextChanged;

        LayerButtonScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            LayerButtonScroll.Offset += new Vector(0, -10 * e.Delta.Y);

            LinesScroll.Offset = new Vector(LinesScroll.Offset.X, LayerButtonScroll.Offset.Y);

            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        TimescaleScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            // タイムラインズーム修飾キーが押されている場合はズーム処理
            if (e.KeyModifiers.HasFlag(_timelineZoomModifier))
            {
                HandleTimelineZoom(e);
            }
            else
            {
                // 通常のスクロール処理
                TimescaleScroll.Offset += new Vector((-10 * e.Delta.Y) + (-10 * e.Delta.X), 0);
                LinesScroll.Offset = new Vector(TimescaleScroll.Offset.X, LinesScroll.Offset.Y);
            }

            // マウスホイールのイベントを無効にする
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);


        LinesScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            // タイムラインズーム修飾キーが押されている場合はズーム処理
            if (e.KeyModifiers.HasFlag(_timelineZoomModifier))
            {
                HandleTimelineZoom(e);
            }
            else
            {
                // 通常のスクロール処理
                LinesScroll.Offset += new Vector((-10 * e.Delta.Y) + (-10 * e.Delta.X), 0);
                TimescaleScroll.Offset = new Vector(LinesScroll.Offset.X, 0);
            }

            // マウスホイールのイベントを無効にする
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        TimescaleScroll.ScrollChanged += OnScrollChanged;
        LinesScroll.ScrollChanged += OnScrollChanged;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isUpdatingScrollFromViewModel) return;
        if (VM is null) return;

        _isUpdatingScrollFromView = true;
        int scrollFrame = (int)(TimescaleScroll.Offset.X / VM.Frame_Per_DIP);
        VM.HorizontalScrollPosition = scrollFrame;
        _isUpdatingScrollFromView = false;
    }

    private void TimecodeCanvas_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (VM is null) return;

        var point = e.GetCurrentPoint(sender as Control);
        if (!point.Properties.IsRightButtonPressed)
        {
            _isDraggingTimeline = true;
        }

        int frame = (int)(point.Position.X / VM.Frame_Per_DIP);
        VM.SeekFrame(Math.Max(0, frame));
    }

    private void TimecodeCanvas_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (!_isDraggingTimeline || VM is null) return;

        var point = e.GetCurrentPoint(sender as Control);
        int frame = (int)(point.Position.X / VM.Frame_Per_DIP);
        VM.SeekFrame(Math.Max(0, frame));
    }

    private void TimecodeCanvas_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        _isDraggingTimeline = false;
    }

    private void TimelineCanvas_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (VM is null) return;

        var point = e.GetCurrentPoint(sender as Control);
        if (!point.Properties.IsRightButtonPressed)
        {
            _isDraggingTimeline = true;
        }

        int frame = (int)(point.Position.X / VM.Frame_Per_DIP);
        VM.SeekFrame(Math.Max(0, frame));
    }

    private void TimelineCanvas_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (!_isDraggingTimeline || VM is null) return;

        var point = e.GetCurrentPoint(sender as Control);
        int frame = (int)(point.Position.X / VM.Frame_Per_DIP);
        VM.SeekFrame(Math.Max(0, frame));
    }

    private void TimelineCanvas_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        _isDraggingTimeline = false;
    }

    private void LoadTimelineZoomModifier()
    {
        // キーバインディングサービスからタイムラインズームの修飾キーを取得
        if (_keyBindingService != null)
        {
            var modifier = _keyBindingService.GetModifierForAction("TimelineZoom");
            if (modifier.HasValue)
            {
                _timelineZoomModifier = modifier.Value;
            }
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribedVM != null)
        {
            _subscribedVM.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedVM = null;
        }

        if (DataContext is TimelineViewModel vm)
        {
            _subscribedVM = vm;
            _subscribedVM.PropertyChanged += OnViewModelPropertyChanged;

            // スクロール位置を復元
            RestoreScrollPosition();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_subscribedVM is null) return;

        if (e.PropertyName == nameof(TimelineViewModel.HorizontalScrollPosition))
        {
            OnHorizontalScrollPositionChanged();
            return;
        }

        if (e.PropertyName != nameof(TimelineViewModel.CursorLeft)) return;

        double cursorLeft = _subscribedVM.CursorLeft;
        double viewportWidth = TimescaleScroll.Viewport.Width;
        double scrollRight = TimescaleScroll.Offset.X + viewportWidth;

        if (cursorLeft >= scrollRight || cursorLeft < TimescaleScroll.Offset.X)
        {
            double newOffset = Math.Max(0, cursorLeft - viewportWidth * 0.1);
            TimescaleScroll.Offset = new Vector(newOffset, TimescaleScroll.Offset.Y);
            LinesScroll.Offset = new Vector(newOffset, LinesScroll.Offset.Y);
        }
    }

    private void OnHorizontalScrollPositionChanged()
    {
        if (_isUpdatingScrollFromView) return;
        if (_isUpdatingScrollFromViewModel) return;
        if (_subscribedVM is null) return;

        RestoreScrollPosition();
    }

    private void RestoreScrollPosition()
    {
        if (_subscribedVM is null) return;

        _isUpdatingScrollFromViewModel = true;
        double scrollOffset = _subscribedVM.HorizontalScrollPosition * _subscribedVM.Frame_Per_DIP;
        TimescaleScroll.Offset = new Vector(scrollOffset, 0);
        LinesScroll.Offset = new Vector(scrollOffset, LinesScroll.Offset.Y);
        _isUpdatingScrollFromViewModel = false;
    }

    private void HandleTimelineZoom(PointerWheelEventArgs e)
    {
        if (VM == null) return;

        // マウスカーソル位置を取得
        var mousePosition = e.GetPosition(this);
        var relativeMouseX = mousePosition.X - 100; // レイヤーボタン幅を引く

        // 現在のスクロール位置を取得
        var currentScrollOffset = TimescaleScroll.Offset.X;

        // マウスカーソル位置に対応するフレーム位置を計算
        double mouseFramePosition = (currentScrollOffset + relativeMouseX) / VM.Frame_Per_DIP;

        // ホイールの回転量に基づいてズーム倍率を計算
        double zoomFactor = 1.0 + (e.Delta.Y * 0.1); // 上回転で拡大、下回転で縮小
        double newFramePerDIP = VM.Frame_Per_DIP * zoomFactor;

        // 最小値・最大値の制限（スライダーと同じ範囲）
        newFramePerDIP = Math.Max(0.1, Math.Min(30.0, newFramePerDIP));

        // ズーム後のスクロール位置を計算（マウスカーソル位置を基準に）
        double newScrollOffset = (mouseFramePosition * newFramePerDIP) - relativeMouseX;
        int scrollFrame = (int)(newScrollOffset / newFramePerDIP);

        // スクロール位置の再計算を抑制
        _isUpdatingScrollFromViewModel = true;

        // 新しい値を設定
        VM.Frame_Per_DIP = newFramePerDIP;

        // スクロール位置を設定
        TimescaleScroll.Offset = new Vector(newScrollOffset, TimescaleScroll.Offset.Y);
        LinesScroll.Offset = new Vector(newScrollOffset, LinesScroll.Offset.Y);

        // スクロール位置をViewModelに反映
        VM.HorizontalScrollPosition = scrollFrame;

        _isUpdatingScrollFromViewModel = false;
    }
}
