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
/// Slider のクリック/ドラッグ操作に連動して InteractionStarted / InteractionCompleted
/// イベントおよびコマンドを発行します。
/// </summary>
public class InspectorSlider : Slider
{
    protected override System.Type StyleKeyOverride => typeof(Slider);

    private bool _interactionActive;
    private IPointer? _interactionPointer;

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

    public InspectorSlider()
    {
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel);
    }

    protected override void OnThumbDragStarted(VectorEventArgs e)
    {
        base.OnThumbDragStarted(e);
        ExecuteInteractionStarted();
    }

    protected override void OnThumbDragCompleted(VectorEventArgs e)
    {
        base.OnThumbDragCompleted(e);
        _interactionPointer = null;
        ExecuteInteractionCompleted();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEnabled || _interactionActive)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _interactionPointer = e.Pointer;
        ExecuteInteractionStarted();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_interactionActive || _interactionPointer is null)
        {
            return;
        }

        if (!ReferenceEquals(e.Pointer, _interactionPointer))
        {
            return;
        }

        _interactionPointer = null;
        ExecuteInteractionCompleted();
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!_interactionActive || _interactionPointer is null)
        {
            return;
        }

        if (!ReferenceEquals(e.Pointer, _interactionPointer))
        {
            return;
        }

        _interactionPointer = null;
        ExecuteInteractionCompleted();
    }

    private void ExecuteInteractionStarted()
    {
        if (_interactionActive)
        {
            return;
        }

        _interactionActive = true;
        RaiseEvent(new RoutedEventArgs(InteractionStartedEvent));

        if (InteractionStartedCommand?.CanExecute(null) == true)
        {
            InteractionStartedCommand.Execute(null);
        }
    }

    private void ExecuteInteractionCompleted()
    {
        if (!_interactionActive)
        {
            return;
        }

        _interactionActive = false;
        RaiseEvent(new RoutedEventArgs(InteractionCompletedEvent));

        if (InteractionCompletedCommand?.CanExecute(null) == true)
        {
            InteractionCompletedCommand.Execute(null);
        }
    }
}
