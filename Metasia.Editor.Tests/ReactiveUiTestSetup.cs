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
