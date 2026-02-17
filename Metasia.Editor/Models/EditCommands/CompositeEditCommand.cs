using System;
using System.Collections.Generic;

namespace Metasia.Editor.Models.EditCommands;

/// <summary>
/// 複数の編集コマンドを一つにまとめるコマンド
/// </summary>
public class CompositeEditCommand : IEditCommand
{
    private readonly List<IEditCommand> _commands;
    private readonly string _description;

    public string Description => _description;

    public CompositeEditCommand(IEnumerable<IEditCommand> commands, string description = "複合編集操作")
    {
        _commands = new List<IEditCommand>(commands);
        _description = description;
    }

    public void Execute()
    {
        foreach (var command in _commands)
        {
            command.Execute();
        }
    }

    public void Undo()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }
}