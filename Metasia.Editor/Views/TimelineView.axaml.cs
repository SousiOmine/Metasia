using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Models.States;
using Microsoft.Extensions.DependencyInjection;

namespace Metasia.Editor.Views;

public partial class TimelineView : UserControl
{
    public double Frame_Per_DIP { get; private set; }

    private TimelineViewModel? VM
    {
        get { return this.DataContext as TimelineViewModel; }

    }

    public TimelineView()
    {
        InitializeComponent();

        LayerButtonScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            LayerButtonScroll.Offset += new Vector(0, -10 * e.Delta.Y);

            LinesScroll.Offset = new Vector(LinesScroll.Offset.X, LayerButtonScroll.Offset.Y);

            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        TimescaleScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            TimescaleScroll.Offset += new Vector((-10 * e.Delta.Y) + (-10 * e.Delta.X), 0);

            LinesScroll.Offset = new Vector(TimescaleScroll.Offset.X, LinesScroll.Offset.Y);

            // マウスホイールのイベントを無効にする
            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);


        LinesScroll.AddHandler(InputElement.PointerWheelChangedEvent, (sender, e) =>
        {
            LinesScroll.Offset += new Vector((-10 * e.Delta.Y) + (-10 * e.Delta.X), 0);

            TimescaleScroll.Offset = new Vector(LinesScroll.Offset.X, 0);

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
}
