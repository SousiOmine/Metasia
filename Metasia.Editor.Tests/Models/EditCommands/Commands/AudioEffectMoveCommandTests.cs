using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Editor.Models.EditCommands.Commands;

namespace Metasia.Editor.Tests.Models.EditCommands.Commands;

[TestFixture]
public class AudioEffectMoveCommandTests
{
    private AudioObject _target = null!;
    private VolumeFadeEffect _first = null!;
    private VolumeFadeEffect _second = null!;
    private VolumeFadeEffect _third = null!;

    [SetUp]
    public void Setup()
    {
        _target = new AudioObject("audio");
        _first = new VolumeFadeEffect { Id = "first" };
        _second = new VolumeFadeEffect { Id = "second" };
        _third = new VolumeFadeEffect { Id = "third" };
        _target.AudioEffects.Add(_first);
        _target.AudioEffects.Add(_second);
        _target.AudioEffects.Add(_third);
    }

    [Test]
    public void Execute_MovesEffectToRequestedIndex()
    {
        var command = new AudioEffectMoveCommand(_target, _first, 2);

        command.Execute();

        Assert.That(_target.AudioEffects.Select(x => x.Id), Is.EqualTo(new[] { "second", "third", "first" }));
    }

    [Test]
    public void Undo_RestoresOriginalOrder()
    {
        var command = new AudioEffectMoveCommand(_target, _first, 2);
        command.Execute();

        command.Undo();

        Assert.That(_target.AudioEffects.Select(x => x.Id), Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public void Description_IsCorrect()
    {
        var command = new AudioEffectMoveCommand(_target, _first, 1);

        Assert.That(command.Description, Is.EqualTo("オーディオエフェクトの移動"));
    }
}
