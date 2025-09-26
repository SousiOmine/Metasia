using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Metasia.Core.Project;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// 設定サービスの実装クラス
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _currentSettings;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsService()
        {
            // 設定ファイルのパスを実行ファイルと同じ位置に設定
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? string.Empty;
            _settingsFilePath = Path.Combine(assemblyDirectory, "settings.json");
            
            // JSONシリアライズオプションを設定
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            // 現在の設定を初期化
            _currentSettings = new AppSettings();
        }

        /// <summary>
        /// 現在の設定を取得する
        /// </summary>
        /// <returns>現在の設定</returns>
        public AppSettings GetCurrentSettings()
        {
            return _currentSettings;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="settings">保存する設定</param>
        /// <returns>非同期操作</returns>
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_settingsFilePath, jsonString);
                _currentSettings = settings.Copy();
            }
            catch (Exception ex)
            {
                // 保存に失敗した場合はログ出力などを行う
                Console.WriteLine($"設定の保存に失敗しました: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <returns>読み込んだ設定</returns>
        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var jsonString = await File.ReadAllTextAsync(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(jsonString, _jsonOptions);
                    if (settings != null)
                    {
                        _currentSettings = settings;
                        return settings;
                    }
                }

                // ファイルが存在しない場合はデフォルト設定で新規作成
                var defaultSettings = new AppSettings();
                defaultSettings.ResetToDefaults();
                await SaveSettingsAsync(defaultSettings);
                _currentSettings = defaultSettings;
                
                // デバッグログを出力
                Debug.WriteLine($"設定ファイルが存在しなかったため、デフォルト設定で新規作成しました: {_settingsFilePath}");
                
                return defaultSettings;
            }
            catch (Exception ex)
            {
                // 読み込みに失敗した場合はログ出力などを行い、デフォルト設定を返す
                Console.WriteLine($"設定の読み込みに失敗しました: {ex.Message}");
                var defaultSettings = new AppSettings();
                defaultSettings.ResetToDefaults();
                await SaveSettingsAsync(defaultSettings);
                _currentSettings = defaultSettings;
                return defaultSettings;
            }
        }

        /// <summary>
        /// 設定をデフォルト値にリセットする
        /// </summary>
        /// <returns>非同期操作</returns>
        public async Task ResetToDefaultsAsync()
        {
            var defaultSettings = new AppSettings();
            defaultSettings.ResetToDefaults();
            await SaveSettingsAsync(defaultSettings);
        }
    }
}
