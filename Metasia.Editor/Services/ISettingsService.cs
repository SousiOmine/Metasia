using System;
using System.Threading.Tasks;
using Metasia.Core.Project;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// 設定サービスのインターフェース
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 現在の設定を取得する
        /// </summary>
        /// <returns>現在の設定</returns>
        AppSettings GetCurrentSettings();

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="settings">保存する設定</param>
        /// <returns>非同期操作</returns>
        Task SaveSettingsAsync(AppSettings settings);

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <returns>読み込んだ設定</returns>
        Task<AppSettings> LoadSettingsAsync();

        /// <summary>
        /// 設定をデフォルト値にリセットする
        /// </summary>
        /// <returns>非同期操作</returns>
        Task ResetToDefaultsAsync();
    }
}
