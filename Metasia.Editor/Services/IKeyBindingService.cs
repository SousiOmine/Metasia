
using System.Collections.Generic;
using Avalonia.Input;

namespace Metasia.Editor.Services
{
    public enum CommandIdentifier
    {
        SaveProject,
        Undo,
        Redo
    }

    public enum InteractionIdentifier
    {
        MultiSelect
    }

    public interface IKeyBindingService
    {
        /// <summary>
        /// Gets the key gesture for a specific command
        /// </summary>
        /// <param name="command">The command identifier</param>
        /// <returns>The key gesture for the command</returns>
        KeyGesture? GetGesture(CommandIdentifier command);

        /// <summary>
        /// Gets the modifier keys for a specific interaction
        /// </summary>
        /// <param name="interaction">The interaction identifier</param>
        /// <returns>The modifier keys for the interaction</returns>
        KeyModifiers GetModifiers(InteractionIdentifier interaction);

        /// <summary>
        /// Loads key bindings from a file
        /// </summary>
        /// <param name="filePath">Path to the key bindings file</param>
        void LoadKeyBindings(string filePath);

        /// <summary>
        /// Saves key bindings to a file
        /// </summary>
        /// <param name="filePath">Path to the key bindings file</param>
        void SaveKeyBindings(string filePath);
    }
}
