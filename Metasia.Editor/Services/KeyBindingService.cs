using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia;
using Avalonia.VisualTree;
using Metasia.Editor.Models.KeyBinding;
using Metasia.Editor.Services.KeyBinding;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Linq;

namespace Metasia.Editor.Services
{
    public class KeyBindingService : IKeyBindingService
    {
        private List<KeyBindingDefinition> _keyBindings { get; } = new List<KeyBindingDefinition>();
        private List<ModifierKeyDefinition> _modifierKeyDefinitions { get; } = new List<ModifierKeyDefinition>();

        private Dictionary<string, ICommand> _commands { get; } = new Dictionary<string, ICommand>();
        private readonly IDefaultKeyBindingProvider _defaultProvider;
        private readonly List<WeakReference<Window>> _targetWindows = new List<WeakReference<Window>>();

        private const string SETTINGS_FILE_NAME = "keybindings.json";

        public KeyBindingService()
        {
            _defaultProvider = DefaultKeyBindingProviderFactory.Create();
            LoadKeyBindings();
        }

        public void ApplyKeyBindings(Window target)
        {
            _targetWindows.Add(new WeakReference<Window>(target));
            RefreshKeyBindings();
        }

        /// <summary>
        /// 現在のターゲットウィンドウのキーバインディングを更新する
        /// </summary>
        public void RefreshKeyBindings()
        {
            // 無効なウィンドウ参照を削除
            _targetWindows.RemoveAll(wr => !wr.TryGetTarget(out _));

            foreach (var targetWindowRef in _targetWindows)
            {
                if (targetWindowRef.TryGetTarget(out var targetWindow))
                {
                    // 既存のキーバインディングをクリア
                    targetWindow.KeyBindings.Clear();

                    // 登録されたコマンドの中から、キーバインディングが定義されているものを適用
                    foreach (var keyBinding in _keyBindings)
                    {
                        if (_commands.TryGetValue(keyBinding.CommandId, out var command) && keyBinding.Gesture is not null)
                        {
                            var avaloniaKeyBinding = new Avalonia.Input.KeyBinding
                            {
                                Gesture = keyBinding.Gesture,
                                Command = new TextInputGuardCommand(targetWindow, command)
                            };
                            targetWindow.KeyBindings.Add(avaloniaKeyBinding);
                        }
                    }
                }
            }
        }

        public void RegisterCommand(string commandId, ICommand command)
        {
            _commands[commandId] = command;
            // コマンドが登録されたらキーバインディングを更新
            RefreshKeyBindings();
        }

        /// <summary>
        /// 指定されたコマンドIDのコマンドを登録解除する
        /// </summary>
        public bool UnregisterCommand(string commandId)
        {
            var result = _commands.Remove(commandId);
            if (result)
            {
                // コマンドが削除されたらキーバインディングを更新
                RefreshKeyBindings();
            }
            return result;
        }

        /// <summary>
        /// すべての登録されたコマンドをクリアする
        /// </summary>
        public void ClearCommands()
        {
            _commands.Clear();
            // すべてのコマンドがクリアされたらキーバインディングも更新
            RefreshKeyBindings();
        }

        /// <summary>
        /// キーバインディング設定を読み込む
        /// </summary>
        private void LoadKeyBindings()
        {
            _keyBindings.Clear();
            _modifierKeyDefinitions.Clear();

            // デフォルトのキーバインディングを設定
            SetDefaultKeyBindings();
            SetDefaultModifierKeys();

            // JSONファイルからカスタムキーバインディングを読み込み
            try
            {
                var settingsPath = GetSettingsFilePath();
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<KeyBindingSettings>(json);

                    if (settings?.KeyBindings is not null)
                    {
                        LoadCustomKeyBindings(settings.KeyBindings);
                    }

                    if (settings?.ModifierKeys is not null)
                    {
                        LoadCustomModifierKeys(settings.ModifierKeys);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"キーバインディング設定の読み込みエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// デフォルトのキーバインディングを設定
        /// </summary>
        private void SetDefaultKeyBindings()
        {
            _keyBindings.AddRange(_defaultProvider.GetDefaultKeyBindings());
        }

        /// <summary>
        /// デフォルトの修飾キー設定
        /// </summary>
        private void SetDefaultModifierKeys()
        {
            _modifierKeyDefinitions.AddRange(_defaultProvider.GetDefaultModifierKeys());
        }

        /// <summary>
        /// カスタムキーバインディングを読み込み
        /// </summary>
        private void LoadCustomKeyBindings(List<KeyBindingDefinitionJson> customKeyBindings)
        {
            foreach (var customBinding in customKeyBindings)
            {
                // 既存のキーバインディングを上書きまたは追加
                var existingBinding = _keyBindings.Find(kb => kb.CommandId == customBinding.CommandId);
                var gesture = ParseKeyGesture(customBinding);

                if (existingBinding is not null)
                {
                    existingBinding.Gesture = gesture;
                }
                else
                {
                    _keyBindings.Add(new KeyBindingDefinition
                    {
                        CommandId = customBinding.CommandId,
                        Gesture = gesture
                    });
                }
            }
        }

        /// <summary>
        /// カスタム修飾キー設定を読み込み
        /// </summary>
        private void LoadCustomModifierKeys(List<ModifierKeyDefinitionJson> customModifierKeys)
        {
            foreach (var customModifier in customModifierKeys)
            {
                // 既存の修飾キー設定を上書きまたは追加
                var existingDefinition = _modifierKeyDefinitions.Find(m => m.ActionId == customModifier.ActionId);

                if (Enum.TryParse<KeyModifiers>(customModifier.Modifier, true, out var modifier))
                {
                    if (existingDefinition is not null)
                    {
                        existingDefinition.Modifier = modifier;
                        existingDefinition.Description = customModifier.Description;
                    }
                    else
                    {
                        _modifierKeyDefinitions.Add(new ModifierKeyDefinition
                        {
                            ActionId = customModifier.ActionId,
                            Modifier = modifier,
                            Description = customModifier.Description
                        });
                    }
                }
            }
        }

        /// <summary>
        /// JSON形式のキーバインディングからKeyGestureを作成
        /// </summary>
        private KeyGesture? ParseKeyGesture(KeyBindingDefinitionJson jsonBinding)
        {
            try
            {
                if (!Enum.TryParse<Key>(jsonBinding.Key, true, out var key))
                {
                    return null;
                }

                var modifiers = KeyModifiers.None;
                foreach (var modifier in jsonBinding.Modifiers)
                {
                    if (Enum.TryParse<KeyModifiers>(modifier, true, out var mod))
                    {
                        modifiers |= mod;
                    }
                }

                return new KeyGesture(key, modifiers);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 設定ファイルのパスを取得
        /// </summary>
        private string GetSettingsFilePath()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var exeDirectory = Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory;
            return Path.Combine(exeDirectory, SETTINGS_FILE_NAME);
        }

        /// <summary>
        /// 現在のキーバインディング設定をJSONファイルに保存
        /// </summary>
        public void SaveKeyBindings()
        {
            try
            {
                var settings = new KeyBindingSettings();

                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.Gesture is not null)
                    {
                        var jsonBinding = new KeyBindingDefinitionJson
                        {
                            CommandId = keyBinding.CommandId,
                            Key = keyBinding.Gesture.Key.ToString(),
                            Modifiers = GetModifierStrings(keyBinding.Gesture.KeyModifiers)
                        };
                        settings.KeyBindings.Add(jsonBinding);
                    }
                }

                // 修飾キー設定も保存
                foreach (var modifierKey in _modifierKeyDefinitions)
                {
                    var jsonModifier = new ModifierKeyDefinitionJson
                    {
                        ActionId = modifierKey.ActionId,
                        Modifier = modifierKey.Modifier.ToString(),
                        Description = modifierKey.Description
                    };
                    settings.ModifierKeys.Add(jsonModifier);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var settingsPath = GetSettingsFilePath();
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"キーバインディング設定の保存エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウ内でテキスト入力要素がフォーカスされているかを判定する
        /// </summary>
        private static bool IsTextInputFocused(Window window)
        {
            var focusedElement = window.FocusManager?.GetFocusedElement();
            if (focusedElement is null)
            {
                return false;
            }

            if (IsTextInputElement(focusedElement))
            {
                return true;
            }

            if (focusedElement is Visual visual)
            {
                foreach (var ancestor in visual.GetVisualAncestors())
                {
                    if (ancestor is IInputElement ancestorElement && IsTextInputElement(ancestorElement))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private static bool IsTextInputElement(IInputElement element)
        {
            return element is TextBox;
        }

        private sealed class TextInputGuardCommand : ICommand
        {
            private readonly Window _window;
            private readonly ICommand _inner;

            public TextInputGuardCommand(Window window, ICommand inner)
            {
                _window = window ?? throw new ArgumentNullException(nameof(window));
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public event EventHandler? CanExecuteChanged
            {
                add => _inner.CanExecuteChanged += value;
                remove => _inner.CanExecuteChanged -= value;
            }

            public bool CanExecute(object? parameter)
            {
                if (IsTextInputFocused(_window))
                {
                    return false;
                }

                return _inner.CanExecute(parameter);
            }

            public void Execute(object? parameter)
            {
                if (IsTextInputFocused(_window))
                {
                    return;
                }

                if (_inner.CanExecute(parameter))
                {
                    _inner.Execute(parameter);
                }
            }
        }
        /// KeyModifiersから文字列リストを取得
        /// </summary>
        private List<string> GetModifierStrings(KeyModifiers modifiers)
        {
            var result = new List<string>();

            if ((modifiers & KeyModifiers.Control) != 0)
                result.Add("Control");
            if ((modifiers & KeyModifiers.Alt) != 0)
                result.Add("Alt");
            if ((modifiers & KeyModifiers.Shift) != 0)
                result.Add("Shift");
            if ((modifiers & KeyModifiers.Meta) != 0)
                result.Add("Meta");

            return result;
        }

        /// <summary>
        /// 指定されたアクションに対応する修飾キーを取得
        /// </summary>
        public KeyModifiers? GetModifierForAction(string actionId)
        {
            var definition = _modifierKeyDefinitions.FirstOrDefault(m => m.ActionId == actionId);
            return definition?.Modifier;
        }

        /// <summary>
        /// 指定された修飾キーが現在押されているかを確認
        /// </summary>
        public bool IsModifierKeyPressed(KeyModifiers modifier, KeyModifiers currentModifiers)
        {
            return (currentModifiers & modifier) == modifier;
        }
    }
}