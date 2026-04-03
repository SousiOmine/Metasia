using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Views.Timeline;

namespace Metasia.Editor.Views.Behaviors;

public class ClipMidpointMarkerBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.PointerPressed += OnPointerPressed;
        AssociatedObject.PointerMoved += OnPointerMoved;
        AssociatedObject.PointerReleased += OnPointerReleased;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.PointerPressed -= OnPointerPressed;
        AssociatedObject.PointerMoved -= OnPointerMoved;
        AssociatedObject.PointerReleased -= OnPointerReleased;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not ClipMidpointMarkerViewModel vm)
        {
            return;
        }

        var clipView = AssociatedObject.FindAncestorOfType<ClipView>();
        if (clipView is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
        {
            return;
        }

        vm.StartDrag(e.GetPosition(clipView).X);
        e.Pointer.Capture(AssociatedObject);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (AssociatedObject?.DataContext is not ClipMidpointMarkerViewModel vm)
        {
            return;
        }

        if (e.Pointer.Captured != AssociatedObject)
        {
            return;
        }

        var clipView = AssociatedObject.FindAncestorOfType<ClipView>();
        if (clipView is null)
        {
            return;
        }

        vm.UpdateDrag(e.GetPosition(clipView).X);
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not ClipMidpointMarkerViewModel vm)
        {
            return;
        }

        if (e.Pointer.Captured != AssociatedObject)
        {
            return;
        }

        var clipView = AssociatedObject.FindAncestorOfType<ClipView>();
        if (clipView is not null)
        {
            vm.EndDrag(e.GetPosition(clipView).X);
        }

        e.Pointer.Capture(null);
        e.Handled = true;
    }
}
