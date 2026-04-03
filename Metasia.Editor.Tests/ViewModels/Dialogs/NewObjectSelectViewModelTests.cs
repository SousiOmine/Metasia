using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Linq;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Dialogs;
using NUnit.Framework;

namespace Metasia.Editor.Tests.ViewModels.Dialogs;

[TestFixture]
public class NewObjectSelectViewModelTests
{
    [Test]
    public void ClipList_ContainsTimelineReferenceObject()
    {
        var viewModel = new NewObjectSelectViewModel(NewObjectSelectViewModel.TargetType.Clip);

        Assert.That(
            viewModel.AvailableObjectTypes.Any(x => x.ObjectType == typeof(TimelineReferenceObject)),
            Is.True);
    }
}
