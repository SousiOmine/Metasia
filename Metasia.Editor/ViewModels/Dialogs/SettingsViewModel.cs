using System;
using System.Reactive;
using System.Threading.Tasks;
using Metasia.Core.Project;
using Metasia.Editor.Services;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs
{
    /// <summary>
    /// 設定ウィンドウのViewModel
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private AppSettings _currentSettings;

        /// <summary>
        /// 現在の設定
        /// </summary>
        public AppSettings CurrentSettings
        {
            get => _currentSettings;
            private set => this.RaiseAndSetIfChanged(ref _currentSettings, value);
        }

        /// <summary>
        /// OKコマンド
        /// </summary>
        public ReactiveCommand<Unit, bool> OkCommand { get; }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ReactiveCommand<Unit, bool> CancelCommand { get; }

        /// <summary>
        /// 適用コマンド
        /// </summary>
        public ReactiveCommand<Unit, Unit> ApplyCommand { get; }

        /// <summary>
        /// デフォルトにリセットコマンド
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetToDefaultsCommand { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settingsService">設定サービス</param>
        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            // 現在の設定を読み込む
            _currentSettings = _settingsService.GetCurrentSettings();

            // コマンドを初期化
            OkCommand = ReactiveCommand.CreateFromTask(ExecuteOkAsync);
            CancelCommand = ReactiveCommand.Create(ExecuteCancel);
            ApplyCommand = ReactiveCommand.CreateFromTask(ExecuteApplyAsync);
            ResetToDefaultsCommand = ReactiveCommand.CreateFromTask(ExecuteResetToDefaultsAsync);
        }

        /// <summary>
        /// OKボタンがクリックされたときの処理
        /// </summary>
        /// <returns>trueを返す</returns>
        private async Task<bool> ExecuteOkAsync()
        {
            await ExecuteApplyAsync();
            return true;
        }

        /// <summary>
        /// キャンセルボタンがクリックされたときの処理
        /// </summary>
        /// <returns>falseを返す</returns>
        private bool ExecuteCancel()
        {
            // 設定を元に戻す
            _currentSettings = _settingsService.GetCurrentSettings();
            this.RaisePropertyChanged(nameof(CurrentSettings));
            return false;
        }

        /// <summary>
        /// 適用ボタンがクリックされたときの処理
        /// </summary>
        private async Task ExecuteApplyAsync()
        {
            try
            {
                await _settingsService.SaveSettingsAsync(CurrentSettings);
            }
            catch (Exception ex)
            {
                // エラーハンドリング（将来的にはエラーメッセージを表示するなど）
                Console.WriteLine($"設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// デフォルトにリセットボタンがクリックされたときの処理
        /// </summary>
        private async Task ExecuteResetToDefaultsAsync()
        {
            try
            {
                await _settingsService.ResetToDefaultsAsync();
                CurrentSettings = _settingsService.GetCurrentSettings();
            }
            catch (Exception ex)
            {
                // エラーハンドリング（将来的にはエラーメッセージを表示するなど）
                Console.WriteLine($"設定のリセットに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 設定を再読み込みする
        /// </summary>
        public async Task ReloadSettingsAsync()
        {
            try
            {
                var loadedSettings = await _settingsService.LoadSettingsAsync();
                CurrentSettings = loadedSettings;
            }
            catch (Exception ex)
            {
                // エラーハンドリング（将来的にはエラーメッセージを表示するなど）
                Console.WriteLine($"設定の読み込みに失敗しました: {ex.Message}");
            }
        }
    }
}
