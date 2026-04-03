using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using System;
using System.Collections.Generic;

namespace Metasia.Editor.Models.EditCommands
{
    public class CompositeCommand : IEditCommand
    {
        public string Description { get; }

        private readonly List<IEditCommand> _commands;

        public CompositeCommand(IEnumerable<IEditCommand> commands, string description = "複合編集操作")
        {
            ArgumentNullException.ThrowIfNull(commands);
            _commands = [.. commands];
            Description = description;
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
}