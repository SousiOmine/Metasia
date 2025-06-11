using System;

namespace Metasia.Editor.Models.KeyBindings
{
    /// <summary>
    /// ショートカットキーを割り当てるアクションを識別するための列挙型
    /// </summary>
    public enum CommandIdentifier
    {
        /// <summary>
        /// プロジェクトの保存
        /// </summary>
        SaveProject,
        
        /// <summary>
        /// 元に戻す
        /// </summary>
        Undo,
        
        /// <summary>
        /// やり直し
        /// </summary>
        Redo,
        
        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        OpenProject,
        
        /// <summary>
        /// 新規プロジェクト作成
        /// </summary>
        NewProject
    }
}