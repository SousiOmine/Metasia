using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Metasia.Editor.Controls;

/// <summary>
/// ユーザーの操作開始・完了を検知できるスライダーコントロール。
/// Slider の Thumb ドラッグ操作に連動して InteractionStarted / InteractionCompleted
/// イベントおよびコマンドを発行します。
/// </summary>
public class InspectorSlider : Slider
{
    protected override System.Type StyleKeyOverride => typeof(Slider);

    public static readonly RoutedEvent<RoutedEventArgs> InteractionStartedEvent =
        RoutedEvent.Register<InspectorSlider, RoutedEventArgs>(
            nameof(InteractionStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> InteractionCompletedEvent =
        RoutedEvent.Register<InspectorSlider, RoutedEventArgs>(
            nameof(InteractionCompleted), RoutingStrategies.Bubble);

    public static readonly StyledProperty<ICommand?> InteractionStartedCommandProperty =
        AvaloniaProperty.Register<InspectorSlider, ICommand?>(nameof(InteractionStartedCommand));

    public static readonly StyledProperty<ICommand?> InteractionCompletedCommandProperty =
        AvaloniaProperty.Register<InspectorSlider, ICommand?>(nameof(InteractionCompletedCommand));

    public event EventHandler<RoutedEventArgs> InteractionStarted
    {
        add => AddHandler(InteractionStartedEvent, value);
        remove => RemoveHandler(InteractionStartedEvent, value);
    }

    public event EventHandler<RoutedEventArgs> InteractionCompleted
    {
        add => AddHandler(InteractionCompletedEvent, value);
        remove => RemoveHandler(InteractionCompletedEvent, value);
    }

    public ICommand? InteractionStartedCommand
    {
        get => GetValue(InteractionStartedCommandProperty);
        set => SetValue(InteractionStartedCommandProperty, value);
    }

    public ICommand? InteractionCompletedCommand
    {
        get => GetValue(InteractionCompletedCommandProperty);
        set => SetValue(InteractionCompletedCommandProperty, value);
    }

    protected override void OnThumbDragStarted(VectorEventArgs e)
    {
        base.OnThumbDragStarted(e);

        RaiseEvent(new RoutedEventArgs(InteractionStartedEvent));

        if (InteractionStartedCommand?.CanExecute(null) == true)
        {
            InteractionStartedCommand.Execute(null);
        }
    }

    protected override void OnThumbDragCompleted(VectorEventArgs e)
    {
        base.OnThumbDragCompleted(e);

        RaiseEvent(new RoutedEventArgs(InteractionCompletedEvent));

        if (InteractionCompletedCommand?.CanExecute(null) == true)
        {
            InteractionCompletedCommand.Execute(null);
        }
    }
}
