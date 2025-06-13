using System;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBindings;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// キーバインディングを管理するサービスのインターフェース
    /// </summary>
    public interface IKeyBindingService
    {
        /// <summary>
        /// 指定されたコマンドに対応するキーの組み合わせを取得する
        /// </summary>
        /// <param name="command">コマンド識別子</param>
        /// <returns>キーの組み合わせ</returns>
        KeyGesture GetGesture(CommandIdentifier command);

        /// <summary>
        /// 指定されたインタラクションに割り当てられた修飾キーを取得する
        /// </summary>
        /// <param name="interaction">インタラクション識別子</param>
        /// <returns>修飾キー</returns>
        KeyModifiers GetModifiers(InteractionIdentifier interaction);

        /// <summary>
        /// 設定ファイルからキーバインディングを読み込む
        /// </summary>
        void LoadKeyBindings();

        /// <summary>
        /// 現在のキーバインディングを設定ファイルに保存する
        /// </summary>
        void SaveKeyBindings();
    }
}