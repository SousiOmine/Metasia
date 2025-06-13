using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBindings;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// キーバインディングを管理するサービスの実装
    /// </summary>
    public class KeyBindingService : IKeyBindingService
    {
        private readonly Dictionary<CommandIdentifier, KeyGesture> _commandGestures;
        private readonly Dictionary<InteractionIdentifier, KeyModifiers> _interactionModifiers;
        private readonly string _keyBindingsFilePath;

        public KeyBindingService()
        {
            _commandGestures = new Dictionary<CommandIdentifier, KeyGesture>();
            _interactionModifiers = new Dictionary<InteractionIdentifier, KeyModifiers>();
            
            // 設定ファイルのパスを決定
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Metasia");
            
            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _keyBindingsFilePath = Path.Combine(appDataPath, "keybindings.json");
            
            // 設定を読み込む
            LoadKeyBindings();
        }

        /// <summary>
        /// 指定されたコマンドに対応するキーの組み合わせを取得する
        /// </summary>
        public KeyGesture GetGesture(CommandIdentifier command)
        {
            if (_commandGestures.TryGetValue(command, out var gesture))
            {
                return gesture;
            }
            
            // 登録されていない場合はデフォルト値を返す
            return GetDefaultGesture(command);
        }

        /// <summary>
        /// 指定されたインタラクションに割り当てられた修飾キーを取得する
        /// </summary>
        public KeyModifiers GetModifiers(InteractionIdentifier interaction)
        {
            if (_interactionModifiers.TryGetValue(interaction, out var modifiers))
            {
                return modifiers;
            }
            
            // 登録されていない場合はデフォルト値を返す
            return GetDefaultModifiers(interaction);
        }

        /// <summary>
        /// 設定ファイルからキーバインディングを読み込む
        /// </summary>
        public void LoadKeyBindings()
        {
            try
            {
                if (File.Exists(_keyBindingsFilePath))
                {
                    string json = File.ReadAllText(_keyBindingsFilePath);
                    var settings = JsonSerializer.Deserialize<KeyBindingSettings>(json);
                    
                    if (settings != null)
                    {
                        // コマンドのキーバインディングを読み込む
                        if (settings.CommandGestures != null)
                        {
                            _commandGestures.Clear();
                            foreach (var item in settings.CommandGestures)
                            {
                                if (Enum.TryParse<CommandIdentifier>(item.Key, out var command) &&
                                    TryParseKeyGesture(item.Value, out var gesture))
                                {
                                    _commandGestures[command] = gesture;
                                }
                            }
                        }
                        
                        // インタラクションの修飾キーを読み込む
                        if (settings.InteractionModifiers != null)
                        {
                            _interactionModifiers.Clear();
                            foreach (var item in settings.InteractionModifiers)
                            {
                                if (Enum.TryParse<InteractionIdentifier>(item.Key, out var interaction) &&
                                    Enum.TryParse<KeyModifiers>(item.Value, out var modifiers))
                                {
                                    _interactionModifiers[interaction] = modifiers;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"キーバインディングの読み込みに失敗しました: {ex.Message}");
            }
            
            // デフォルト値を設定
            SetDefaultKeyBindings();
        }

        /// <summary>
        /// 現在のキーバインディングを設定ファイルに保存する
        /// </summary>
        public void SaveKeyBindings()
        {
            try
            {
                var settings = new KeyBindingSettings
                {
                    CommandGestures = new Dictionary<string, string>(),
                    InteractionModifiers = new Dictionary<string, string>()
                };
                
                // コマンドのキーバインディングを保存
                foreach (var item in _commandGestures)
                {
                    settings.CommandGestures[item.Key.ToString()] = item.Value.ToString();
                }
                
                // インタラクションの修飾キーを保存
                foreach (var item in _interactionModifiers)
                {
                    settings.InteractionModifiers[item.Key.ToString()] = item.Value.ToString();
                }
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(_keyBindingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"キーバインディングの保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// デフォルトのキーバインディングを設定する
        /// </summary>
        private void SetDefaultKeyBindings()
        {
            // コマンドのデフォルトキーバインディングを設定
            foreach (CommandIdentifier command in Enum.GetValues(typeof(CommandIdentifier)))
            {
                if (!_commandGestures.ContainsKey(command))
                {
                    _commandGestures[command] = GetDefaultGesture(command);
                }
            }
            
            // インタラクションのデフォルト修飾キーを設定
            foreach (InteractionIdentifier interaction in Enum.GetValues(typeof(InteractionIdentifier)))
            {
                if (!_interactionModifiers.ContainsKey(interaction))
                {
                    _interactionModifiers[interaction] = GetDefaultModifiers(interaction);
                }
            }
        }

        /// <summary>
        /// 指定されたコマンドのデフォルトのキーの組み合わせを取得する
        /// </summary>
        private KeyGesture GetDefaultGesture(CommandIdentifier command)
        {
            return command switch
            {
                CommandIdentifier.SaveProject => new KeyGesture(Key.S, KeyModifiers.Control),
                CommandIdentifier.Undo => new KeyGesture(Key.Z, KeyModifiers.Control),
                CommandIdentifier.Redo => new KeyGesture(Key.Y, KeyModifiers.Control),
                CommandIdentifier.OpenProject => new KeyGesture(Key.O, KeyModifiers.Control),
                CommandIdentifier.NewProject => new KeyGesture(Key.N, KeyModifiers.Control),
                _ => new KeyGesture(Key.None)
            };
        }

        /// <summary>
        /// 指定されたインタラクションのデフォルトの修飾キーを取得する
        /// </summary>
        private KeyModifiers GetDefaultModifiers(InteractionIdentifier interaction)
        {
            return interaction switch
            {
                InteractionIdentifier.MultiSelect => KeyModifiers.Control,
                _ => KeyModifiers.None
            };
        }

        /// <summary>
        /// 文字列からKeyGestureを解析する
        /// </summary>
        private bool TryParseKeyGesture(string gestureString, out KeyGesture gesture)
        {
            gesture = null;
            
            try
            {
                gesture = KeyGesture.Parse(gestureString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// キーバインディング設定のJSONシリアライズ用クラス
    /// </summary>
    internal class KeyBindingSettings
    {
        public Dictionary<string, string> CommandGestures { get; set; }
        public Dictionary<string, string> InteractionModifiers { get; set; }
    }
}