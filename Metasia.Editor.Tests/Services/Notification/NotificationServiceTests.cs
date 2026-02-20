using Metasia.Editor.Services.Notification;

namespace Metasia.Editor.Tests.Services.Notification;

[TestFixture]
public class NotificationServiceTests
{
    private NotificationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new NotificationService();
    }

    [Test]
    public void Show_AddsNotificationToList()
    {
        _service.Show("Title", "Message");

        Assert.That(_service.Notifications, Has.Count.EqualTo(1));
    }

    [Test]
    public void Show_RaisesNewNotificationEvent()
    {
        NotificationItem? raisedNotification = null;
        _service.NewNotification += (sender, item) => raisedNotification = item;

        _service.Show("Title", "Message");

        Assert.That(raisedNotification, Is.Not.Null);
        Assert.That(raisedNotification!.Title, Is.EqualTo("Title"));
        Assert.That(raisedNotification.Message, Is.EqualTo("Message"));
    }

    [Test]
    public void Show_SetsCorrectProperties()
    {
        Action? onClick = () => { };
        _service.Show("Test Title", "Test Message", NotificationSeverity.Warning, onClick);

        var notification = _service.Notifications[0];
        Assert.That(notification.Title, Is.EqualTo("Test Title"));
        Assert.That(notification.Message, Is.EqualTo("Test Message"));
        Assert.That(notification.Severity, Is.EqualTo(NotificationSeverity.Warning));
        Assert.That(notification.OnClick, Is.SameAs(onClick));
    }

    [Test]
    public void ShowInfo_SetsInfoSeverity()
    {
        _service.ShowInfo("Title", "Message");

        Assert.That(_service.Notifications[0].Severity, Is.EqualTo(NotificationSeverity.Info));
    }

    [Test]
    public void ShowSuccess_SetsSuccessSeverity()
    {
        _service.ShowSuccess("Title", "Message");

        Assert.That(_service.Notifications[0].Severity, Is.EqualTo(NotificationSeverity.Success));
    }

    [Test]
    public void ShowWarning_SetsWarningSeverity()
    {
        _service.ShowWarning("Title", "Message");

        Assert.That(_service.Notifications[0].Severity, Is.EqualTo(NotificationSeverity.Warning));
    }

    [Test]
    public void ShowError_SetsErrorSeverity()
    {
        _service.ShowError("Title", "Message");

        Assert.That(_service.Notifications[0].Severity, Is.EqualTo(NotificationSeverity.Error));
    }

    [Test]
    public void Remove_RemovesNotification()
    {
        _service.Show("Title", "Message");
        var notification = _service.Notifications[0];

        _service.Remove(notification);

        Assert.That(_service.Notifications, Is.Empty);
    }

    [Test]
    public void Remove_RaisesNotificationRemovedEvent()
    {
        _service.Show("Title", "Message");
        var notification = _service.Notifications[0];
        NotificationItem? removedNotification = null;
        _service.NotificationRemoved += (sender, item) => removedNotification = item;

        _service.Remove(notification);

        Assert.That(removedNotification, Is.SameAs(notification));
    }

    [Test]
    public void Remove_NonExistentNotification_NoEvent()
    {
        var fakeNotification = new NotificationItem("Fake", "Fake", NotificationSeverity.Info);
        var eventRaised = false;
        _service.NotificationRemoved += (sender, item) => eventRaised = true;

        _service.Remove(fakeNotification);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void Clear_RemovesAllNotifications()
    {
        _service.Show("Title1", "Message1");
        _service.Show("Title2", "Message2");
        _service.Show("Title3", "Message3");

        _service.Clear();

        Assert.That(_service.Notifications, Is.Empty);
    }

    [Test]
    public void Clear_RaisesRemovedEventForEach()
    {
        _service.Show("Title1", "Message1");
        _service.Show("Title2", "Message2");
        var removedNotifications = new List<NotificationItem>();
        _service.NotificationRemoved += (sender, item) => removedNotifications.Add(item);

        _service.Clear();

        Assert.That(removedNotifications, Has.Count.EqualTo(2));
    }

    [Test]
    public void Notifications_ReturnsCorrectList()
    {
        _service.Show("Title1", "Message1");
        _service.Show("Title2", "Message2");

        var notifications = _service.Notifications;

        Assert.That(notifications, Has.Count.EqualTo(2));
        Assert.That(notifications[0].Title, Is.EqualTo("Title1"));
        Assert.That(notifications[1].Title, Is.EqualTo("Title2"));
    }

    [Test]
    public void OnClick_CanBeExecuted()
    {
        var clickExecuted = false;
        Action onClick = () => clickExecuted = true;
        _service.Show("Title", "Message", NotificationSeverity.Info, onClick);

        _service.Notifications[0].OnClick?.Invoke();

        Assert.That(clickExecuted, Is.True);
    }
}