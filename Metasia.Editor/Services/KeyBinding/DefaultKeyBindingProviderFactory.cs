using System.Runtime.InteropServices;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// プラットフォームに応じたデフォルトキーバインディングプロバイダーを生成するファクトリー
    /// </summary>
    public static class DefaultKeyBindingProviderFactory
    {
        /// <summary>
        /// 現在のプラットフォームに適したキーバインディングプロバイダーを作成
        /// </summary>
        /// <returns>プラットフォーム固有のキーバインディングプロバイダー</returns>
        public static IDefaultKeyBindingProvider Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsDefaultKeyBindingProvider();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacDefaultKeyBindingProvider();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxDefaultKeyBindingProvider();
            }
            else
            {
                // 不明なプラットフォームの場合はWindowsをフォールバックとして使用
                return new WindowsDefaultKeyBindingProvider();
            }
        }

        /// <summary>
        /// 現在のプラットフォーム名を取得（デバッグ用）
        /// </summary>
        /// <returns>プラットフォーム名</returns>
        public static string GetCurrentPlatformName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            else
                return "Unknown";
        }
    }
} 