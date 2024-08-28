using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace Metasia.Editor.Views;

public partial class TimelineView : UserControl
{
    public double Frame_Per_DIP = 1.0;
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
}