using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Metasia.Core.Project
{
    /// <summary>
    /// アプリケーション設定を保持するクラス
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public AppSettings()
        {
            // 現時点では設定項目は空
            // 将来的に設定項目を追加する場合はここにプロパティを追加
        }

        /// <summary>
        /// 設定をコピーするメソッド
        /// </summary>
        /// <returns>設定のコピー</returns>
        public AppSettings Copy()
        {
            return new AppSettings();
        }

        /// <summary>
        /// 設定をデフォルト値にリセットするメソッド
        /// </summary>
        public void ResetToDefaults()
        {
            // 現時点ではリセットする項目はない
            // 将来的に設定項目を追加した場合はここでデフォルト値を設定
        }
    }
}
