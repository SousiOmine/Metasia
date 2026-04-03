using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands;

[TestFixture]
public class AudioEffectIsActiveChangeCommandTests
{
    private AudioObject _target = null!;
    private VolumeFadeEffect _effect = null!;

    [SetUp]
    public void Setup()
    {
        _target = new AudioObject("audio");
        _effect = new VolumeFadeEffect { Id = "effect", IsActive = true };
        _target.AudioEffects.Add(_effect);
    }

    [Test]
    public void Execute_ChangesIsActive()
    {
        var command = new AudioEffectIsActiveChangeCommand(_target, _effect, false);

        command.Execute();

        Assert.That(_effect.IsActive, Is.False);
    }

    [Test]
    public void Undo_RestoresOriginalState()
    {
        var command = new AudioEffectIsActiveChangeCommand(_target, _effect, false);
        command.Execute();

        command.Undo();

        Assert.That(_effect.IsActive, Is.True);
    }

    [Test]
    public void Description_IsCorrect()
    {
        var command = new AudioEffectIsActiveChangeCommand(_target, _effect, false);

        Assert.That(command.Description, Is.EqualTo("オーディオエフェクトの有効状態変更"));
    }
}
