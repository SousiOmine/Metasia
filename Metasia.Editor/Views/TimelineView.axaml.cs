using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Models.States;
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

    public TimelineView()
    {
        InitializeComponent();

        // キーバインディングサービスを取得
        _keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
        LoadTimelineZoomModifier();

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
    }

    private void TimecodeCanvas_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        int frame = (int)(point.Position.X / VM.Frame_Per_DIP);
        VM.SeekFrame(frame);
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

        // 新しい値を設定
        VM.Frame_Per_DIP = newFramePerDIP;

        // ズーム後のスクロール位置を計算（マウスカーソル位置を基準に）
        double newScrollOffset = (mouseFramePosition * newFramePerDIP) - relativeMouseX;

        // スクロール位置を設定
        TimescaleScroll.Offset = new Vector(newScrollOffset, TimescaleScroll.Offset.Y);
        LinesScroll.Offset = new Vector(newScrollOffset, LinesScroll.Offset.Y);
    }
}
