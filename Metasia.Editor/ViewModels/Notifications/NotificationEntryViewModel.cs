using System;
using System.Windows.Input;
using Avalonia.Media;
using Metasia.Editor.Services.Notification;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Notifications;

public sealed class NotificationEntryViewModel : ViewModelBase
{
    private static readonly SolidColorBrush InfoAccentBrush = new(Color.Parse("#2563EB"));
    private static readonly SolidColorBrush InfoBackgroundBrush = new(Color.Parse("#EFF6FF"));
    private static readonly SolidColorBrush SuccessAccentBrush = new(Color.Parse("#15803D"));
    private static readonly SolidColorBrush SuccessBackgroundBrush = new(Color.Parse("#F0FDF4"));
    private static readonly SolidColorBrush WarningAccentBrush = new(Color.Parse("#C2410C"));
    private static readonly SolidColorBrush WarningBackgroundBrush = new(Color.Parse("#FFF7ED"));
    private static readonly SolidColorBrush ErrorAccentBrush = new(Color.Parse("#B91C1C"));
    private static readonly SolidColorBrush ErrorBackgroundBrush = new(Color.Parse("#FEF2F2"));
    private static readonly SolidColorBrush NeutralBorderBrush = new(Color.Parse("#D1D5DB"));

    private readonly Action _activateAction;

    public Guid Id => Item.Id;
    public NotificationItem Item { get; }
    public string Title => Item.Title;
    public string Message => Item.Message;
    public string FullTimestampText => Item.Timestamp.ToString("yyyy/MM/dd HH:mm:ss");
    public ICommand ActivateCommand { get; }
    public IBrush AccentBrush { get; }
    public IBrush BackgroundBrush { get; }
    public IBrush BorderBrush => NeutralBorderBrush;

    public NotificationEntryViewModel(NotificationItem item, Action activateAction)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        _activateAction = activateAction ?? throw new ArgumentNullException(nameof(activateAction));
        ActivateCommand = ReactiveCommand.Create(Activate);

        (AccentBrush, BackgroundBrush) = Item.Severity switch
        {
            NotificationSeverity.Info => (InfoAccentBrush, InfoBackgroundBrush),
            NotificationSeverity.Success => (SuccessAccentBrush, SuccessBackgroundBrush),
            NotificationSeverity.Warning => (WarningAccentBrush, WarningBackgroundBrush),
            NotificationSeverity.Error => (ErrorAccentBrush, ErrorBackgroundBrush),
            _ => (InfoAccentBrush, InfoBackgroundBrush)
        };
    }

    private void Activate()
    {
        Item.OnClick?.Invoke();
        _activateAction();
    }
}
