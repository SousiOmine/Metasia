using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.ViewModels.Notifications;

namespace Metasia.Editor.Tests.ViewModels.Notifications;

[TestFixture]
public class NotificationCenterViewModelTests
{
    [Test]
    public void Show_AddsNotificationToHistoryAndActiveToasts()
    {
        var service = new NotificationService();
        using var viewModel = new NotificationCenterViewModel(service, TimeSpan.FromSeconds(1), 4);

        service.ShowInfo("Title", "Message");

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.History, Has.Count.EqualTo(1));
            Assert.That(viewModel.ActiveToasts, Has.Count.EqualTo(1));
            Assert.That(viewModel.History[0].Title, Is.EqualTo("Title"));
            Assert.That(viewModel.ActiveToasts[0].Message, Is.EqualTo("Message"));
        });
    }

    [Test]
    public async Task AutoDismiss_RemovesToastButKeepsHistory()
    {
        var service = new NotificationService();
        using var viewModel = new NotificationCenterViewModel(service, TimeSpan.FromMilliseconds(50), 4);

        service.ShowInfo("Title", "Message");
        await Task.Delay(200);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.History, Has.Count.EqualTo(1));
            Assert.That(viewModel.ActiveToasts, Is.Empty);
        });
    }

    [Test]
    public void Show_MoreThanMaxActiveToasts_LimitsVisibleToastCount()
    {
        var service = new NotificationService();
        using var viewModel = new NotificationCenterViewModel(service, TimeSpan.FromSeconds(30), 4);

        for (int i = 1; i <= 5; i++)
        {
            service.ShowInfo($"Title{i}", $"Message{i}");
        }

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.History, Has.Count.EqualTo(5));
            Assert.That(viewModel.ActiveToasts, Has.Count.EqualTo(4));
            Assert.That(viewModel.ActiveToasts[0].Title, Is.EqualTo("Title5"));
            Assert.That(viewModel.ActiveToasts[^1].Title, Is.EqualTo("Title2"));
        });
    }

    [Test]
    public void ActivateCommand_ExecutesNotificationActionAndRemovesToast()
    {
        var service = new NotificationService();
        using var viewModel = new NotificationCenterViewModel(service, TimeSpan.FromSeconds(30), 4);
        var invoked = false;

        service.Show("Title", "Message", NotificationSeverity.Info, () => invoked = true);
        viewModel.ActiveToasts[0].ActivateCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(invoked, Is.True);
            Assert.That(viewModel.History, Has.Count.EqualTo(1));
            Assert.That(viewModel.ActiveToasts, Is.Empty);
        });
    }
}
