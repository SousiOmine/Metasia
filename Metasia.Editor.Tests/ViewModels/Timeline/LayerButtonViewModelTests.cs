using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Timeline;
using Moq;
using NUnit.Framework;

namespace Metasia.Editor.Tests.ViewModels.Timeline;

[TestFixture]
public class LayerButtonViewModelTests
{
    [Test]
    public void ButtonClick_TogglesLayerActiveState_WithoutChangingSelection()
    {
        var layer = new LayerObject("layer-0", "Layer 0")
        {
            IsActive = true
        };
        var editCommandManager = new EditCommandManager();
        var projectStateMock = new Mock<IProjectState>();
        var selectionState = new SelectionState();

        using var viewModel = new LayerButtonViewModel(
            layer,
            editCommandManager,
            projectStateMock.Object,
            selectionState);

        viewModel.ButtonClick.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(layer.IsActive, Is.False);
            Assert.That(selectionState.SelectedLayer, Is.Null);
        });
    }
}
