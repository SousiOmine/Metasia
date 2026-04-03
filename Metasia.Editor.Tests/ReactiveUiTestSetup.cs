using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using NUnit.Framework;
using ReactiveUI.Builder;

namespace Metasia.Editor.Tests;

[SetUpFixture]
public sealed class ReactiveUiTestSetup
{
    [OneTimeSetUp]
    public void InitializeReactiveUi()
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
