using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands;

[TestFixture]
public class VisualEffectMoveCommandTests
{
    private Text _target = null!;
    private BorderEffect _first = null!;
    private ClippingEffect _second = null!;
    private FlipEffect _third = null!;

    [SetUp]
    public void Setup()
    {
        _target = new Text("text");
        _first = new BorderEffect { Id = "first" };
        _second = new ClippingEffect { Id = "second" };
        _third = new FlipEffect { Id = "third" };
        _target.VisualEffects.Add(_first);
        _target.VisualEffects.Add(_second);
        _target.VisualEffects.Add(_third);
    }

    [Test]
    public void Execute_MovesEffectToRequestedIndex()
    {
        var command = new VisualEffectMoveCommand(_target, _first, 2);

        command.Execute();

        Assert.That(_target.VisualEffects.Select(x => x.Id), Is.EqualTo(new[] { "second", "third", "first" }));
    }

    [Test]
    public void Undo_RestoresOriginalOrder()
    {
        var command = new VisualEffectMoveCommand(_target, _first, 2);
        command.Execute();

        command.Undo();

        Assert.That(_target.VisualEffects.Select(x => x.Id), Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public void Description_IsCorrect()
    {
        var command = new VisualEffectMoveCommand(_target, _first, 1);

        Assert.That(command.Description, Is.EqualTo("ビジュアルエフェクトの移動"));
    }
}
