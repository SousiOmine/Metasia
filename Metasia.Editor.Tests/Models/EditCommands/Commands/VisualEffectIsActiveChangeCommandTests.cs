using Metasia.Core.Objects;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands;

[TestFixture]
public class VisualEffectIsActiveChangeCommandTests
{
    private Text _target = null!;
    private BorderEffect _effect = null!;

    [SetUp]
    public void Setup()
    {
        _target = new Text("text");
        _effect = new BorderEffect { Id = "effect", IsActive = true };
        _target.VisualEffects.Add(_effect);
    }

    [Test]
    public void Execute_ChangesIsActive()
    {
        var command = new VisualEffectIsActiveChangeCommand(_target, _effect, false);

        command.Execute();

        Assert.That(_effect.IsActive, Is.False);
    }

    [Test]
    public void Undo_RestoresOriginalState()
    {
        var command = new VisualEffectIsActiveChangeCommand(_target, _effect, false);
        command.Execute();

        command.Undo();

        Assert.That(_effect.IsActive, Is.True);
    }

    [Test]
    public void Description_IsCorrect()
    {
        var command = new VisualEffectIsActiveChangeCommand(_target, _effect, false);

        Assert.That(command.Description, Is.EqualTo("ビジュアルエフェクトの有効状態変更"));
    }
}
