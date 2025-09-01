using Metasia.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class ClipsSplitCommand : IEditCommand
    {
        public string Description => "クリップの分割";

        private readonly List<ClipObject> _targetClips;
        private readonly List<LayerObject> _ownerLayers;
        private readonly int _splitFrame;
        private readonly List<ClipObject> _firstClips = new();
        private readonly List<ClipObject> _secondClips = new();
        private readonly List<int> _objectIndices = new();

        public ClipsSplitCommand(IEnumerable<ClipObject> targetClips, IEnumerable<LayerObject> ownerLayers, int splitFrame)
        {
            // nullチェック
            if (targetClips == null)
            {
                throw new ArgumentNullException(nameof(targetClips), "分割対象のクリップコレクションがnullです。");
            }
            
            if (ownerLayers == null)
            {
                throw new ArgumentNullException(nameof(ownerLayers), "所有レイヤーコレクションがnullです。");
            }
            
            // リストに変換
            _targetClips = targetClips.ToList();
            _ownerLayers = ownerLayers.ToList();
            
            // コレクションの数を検証
            if (_targetClips.Count == 0)
            {
                throw new ArgumentException("分割対象のクリップがありません。", nameof(targetClips));
            }
            
            if (_ownerLayers.Count == 0)
            {
                throw new ArgumentException("所有レイヤーがありません。", nameof(ownerLayers));
            }
            
            if (_targetClips.Count != _ownerLayers.Count)
            {
                throw new ArgumentException("クリップとレイヤーの数が一致しません。", nameof(ownerLayers));
            }
            
            // 分割フレームの検証（非負であることを確認）
            if (splitFrame < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(splitFrame), "分割フレームは0以上である必要があります。");
            }
            
            _splitFrame = splitFrame;

            // 各クリップの元のインデックスを保存
            for (int i = 0; i < _targetClips.Count; i++)
            {
                _objectIndices.Add(_ownerLayers[i].Objects.IndexOf(_targetClips[i]));
            }
        }

        public void Execute()
        {
            // すべてのクリップが分割可能か検証
            for (int i = 0; i < _targetClips.Count; i++)
            {
                var targetClip = _targetClips[i];
                if (!IsClipSplittable(targetClip))
                {
                    throw new ArgumentException($"クリップ '{targetClip.Id}' はフレーム {_splitFrame} で分割できません。");
                }
            }
            
            // すべてのクリップを分割（TimelineViewModelで事前にフィルタリング済み）
            for (int i = 0; i < _targetClips.Count; i++)
            {
                var targetClip = _targetClips[i];
                var ownerLayer = _ownerLayers[i];
                
                // クリップを分割
                var splitResult = targetClip.SplitAtFrame(_splitFrame);
                
                // 分割結果の各要素がnullかチェック
                if (splitResult.Item1 is null || splitResult.Item2 is null)
                {
                    throw new InvalidOperationException($"クリップ '{targetClip.Id}' のフレーム {_splitFrame} での分割に失敗しました。分割結果のクリップがnullです。");
                }
                
                var firstClip = splitResult.Item1;
                var secondClip = splitResult.Item2;

                _firstClips.Add(firstClip);
                _secondClips.Add(secondClip);

                // 元のクリップを削除
                if (ownerLayer.Objects.Contains(targetClip))
                {
                    ownerLayer.Objects.Remove(targetClip);
                }

                // 分割されたクリップを追加
                int originalIndex = _objectIndices[i];
                if (originalIndex >= 0 && originalIndex <= ownerLayer.Objects.Count)
                {
                    ownerLayer.Objects.Insert(originalIndex, firstClip);
                    ownerLayer.Objects.Insert(originalIndex + 1, secondClip);
                }
                else
                {
                    ownerLayer.Objects.Add(firstClip);
                    ownerLayer.Objects.Add(secondClip);
                }
            }
        }

        public void Undo()
        {
            // 逆順に処理してインデックスの問題を防ぐ
            for (int i = _targetClips.Count - 1; i >= 0; i--)
            {
                var targetClip = _targetClips[i];
                var ownerLayer = _ownerLayers[i];
                var firstClip = _firstClips[i];
                var secondClip = _secondClips[i];

                // 分割されたクリップを削除
                if (ownerLayer.Objects.Contains(firstClip))
                {
                    ownerLayer.Objects.Remove(firstClip);
                }
                if (ownerLayer.Objects.Contains(secondClip))
                {
                    ownerLayer.Objects.Remove(secondClip);
                }

                // 元のクリップを元の位置に戻す
                int originalIndex = _objectIndices[i];
                if (originalIndex >= 0 && originalIndex <= ownerLayer.Objects.Count)
                {
                    ownerLayer.Objects.Insert(originalIndex, targetClip);
                }
                else
                {
                    ownerLayer.Objects.Add(targetClip);
                }
            }
        }

        /// <summary>
        /// クリップが指定されたフレームで分割可能かどうかを判定します
        /// </summary>
        /// <param name="clip">対象のクリップ</param>
        /// <returns>分割可能な場合はtrue、そうでない場合はfalse</returns>
        private bool IsClipSplittable(ClipObject clip)
        {
            return _splitFrame > clip.StartFrame && _splitFrame < clip.EndFrame;
        }
    }
}