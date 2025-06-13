

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Input;

namespace Metasia.Editor.Services
{
    public class KeyBindingService : IKeyBindingService
    {
        private readonly Dictionary<CommandIdentifier, KeyGesture> _commandBindings = new();
        private readonly Dictionary<InteractionIdentifier, KeyModifiers> _interactionBindings = new();
        private const string DefaultKeyBindingsFile = "keybindings.json";

        public KeyBindingService()
        {
            // Set default key bindings
            _commandBindings[CommandIdentifier.SaveProject] = new KeyGesture(Key.S, KeyModifiers.Control);
            _commandBindings[CommandIdentifier.Undo] = new KeyGesture(Key.Z, KeyModifiers.Control);
            _commandBindings[CommandIdentifier.Redo] = new KeyGesture(Key.Y, KeyModifiers.Control);

            _interactionBindings[InteractionIdentifier.MultiSelect] = KeyModifiers.Control;

            // Try to load key bindings from file
            LoadKeyBindings(DefaultKeyBindingsFile);
        }

        public KeyGesture? GetGesture(CommandIdentifier command)
        {
            if (_commandBindings.TryGetValue(command, out var gesture))
            {
                return gesture;
            }
            return null;
        }

        public KeyModifiers GetModifiers(InteractionIdentifier interaction)
        {
            if (_interactionBindings.TryGetValue(interaction, out var modifiers))
            {
                return modifiers;
            }
            return KeyModifiers.None;
        }

        public void LoadKeyBindings(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var data = JsonSerializer.Deserialize<KeyBindingData>(json);

                    if (data != null)
                    {
                        // Load command bindings
                        foreach (var binding in data.CommandBindings)
                        {
                            if (Enum.TryParse(binding.Command, out CommandIdentifier command) &&
                                TryParseKeyGesture(binding.Gesture, out var gesture))
                            {
                                _commandBindings[command] = gesture;
                            }
                        }

                        // Load interaction bindings
                        foreach (var binding in data.InteractionBindings)
                        {
                            if (Enum.TryParse(binding.Interaction, out InteractionIdentifier interaction) &&
                                Enum.TryParse(binding.Modifiers, out KeyModifiers modifiers))
                            {
                                _interactionBindings[interaction] = modifiers;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading key bindings: {ex.Message}");
                }
            }
        }

        public void SaveKeyBindings(string filePath)
        {
            var data = new KeyBindingData
            {
                CommandBindings = new List<CommandBinding>(),
                InteractionBindings = new List<InteractionBinding>()
            };

            // Save command bindings
            foreach (var binding in _commandBindings)
            {
                data.CommandBindings.Add(new CommandBinding
                {
                    Command = binding.Key.ToString(),
                    Gesture = binding.Value.ToString()
                });
            }

            // Save interaction bindings
            foreach (var binding in _interactionBindings)
            {
                data.InteractionBindings.Add(new InteractionBinding
                {
                    Interaction = binding.Key.ToString(),
                    Modifiers = binding.Value.ToString()
                });
            }

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving key bindings: {ex.Message}");
            }
        }

        private bool TryParseKeyGesture(string gestureString, out KeyGesture? gesture)
        {
            gesture = null;
            try
            {
                // Format: "Key+Modifiers" or just "Key"
                var parts = gestureString.Split('+');
                if (parts.Length == 1)
                {
                    // Just a key
                    if (Enum.TryParse(parts[0].Trim(), out Key key))
                    {
                        gesture = new KeyGesture(key);
                        return true;
                    }
                }
                else if (parts.Length == 2)
                {
                    // Key + Modifiers
                    if (Enum.TryParse(parts[0].Trim(), out Key key) &&
                        Enum.TryParse(parts[1].Trim(), out KeyModifiers modifiers))
                    {
                        gesture = new KeyGesture(key, modifiers);
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return false;
        }

        private class KeyBindingData
        {
            public List<CommandBinding> CommandBindings { get; set; } = new();
            public List<InteractionBinding> InteractionBindings { get; set; } = new();
        }

        private class CommandBinding
        {
            public string Command { get; set; } = string.Empty;
            public string Gesture { get; set; } = string.Empty;
        }

        private class InteractionBinding
        {
            public string Interaction { get; set; } = string.Empty;
            public string Modifiers { get; set; } = string.Empty;
        }
    }
}

