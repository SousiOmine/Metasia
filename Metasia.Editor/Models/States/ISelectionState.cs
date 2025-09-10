using System;
using System.Collections.Generic;
using Metasia.Core.Objects;


namespace Metasia.Editor.Models.States;

public interface ISelectionState
{
    /// <summary>
    /// 選択されているクリップのリスト
    /// </summary>
    IReadOnlyList<ClipObject> SelectedClips { get; }

    /// <summary>
    /// 選択されているクリップのうち、最初に選択されたクリップ
    /// </summary>
    ClipObject? CurrentSelectedClip { get; }

    /// <summary>
    /// 指定したクリップを選択する
    /// </summary>
    /// <param name="clip">選択するクリップ</param>
    void SelectClip(ClipObject clip);

    /// <summary>
    /// 指定したクリップのリストを選択する
    /// </summary>
    /// <param name="clips">選択するクリップのリスト</param>
    void SelectClips(List<ClipObject> clips);

    /// <summary>
    /// 指定したクリップを選択解除する
    /// </summary>
    /// <param name="clip">選択解除するクリップ</param>
    void UnselectClip(ClipObject clip);

    /// <summary>
    /// 指定したクリップのリストを選択解除する
    /// </summary>
    /// <param name="clips">選択解除するクリップのリスト</param>
    void UnselectClips(IEnumerable<ClipObject> clips);

    /// <summary>
    /// 選択されているクリップをクリアする
    /// </summary>
    void ClearSelectedClips();

    /// <summary>
    /// 選択されているクリップが変更された時に発生するイベント
    /// </summary>
    event Action? SelectionChanged;
}