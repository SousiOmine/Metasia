using Metasia.Core.Coordinate;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// グループ制御オブジェクト - タイムライン上に配置され、
    /// 指定した範囲のレイヤーの下にあるオブジェクトに影響を与える
    /// </summary>
    public class GroupControlObject : MetasiaObject, IMetaCoordable
    {
        /// <summary>
        /// グループ名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 制御対象とする下位レイヤーの数（何段下まで制御するか）
        /// </summary>
        public int TargetLayerDepth { get; set; } = 2;

        // IMetaCoordable の実装 - グループ全体への変換
        public MetaDoubleParam X { get; set; }
        public MetaDoubleParam Y { get; set; }
        public MetaDoubleParam Scale { get; set; }
        public MetaDoubleParam Alpha { get; set; }
        public MetaDoubleParam Rotation { get; set; }

        /// <summary>
        /// グループ制御の合成モード
        /// </summary>
        public GroupBlendMode BlendMode { get; set; } = GroupBlendMode.Normal;

        /// <summary>
        /// グループ制御のエフェクト適用モード
        /// </summary>
        public GroupEffectMode EffectMode { get; set; } = GroupEffectMode.Multiplicative;

        public GroupControlObject(string id, string groupName) : base(id)
        {
            GroupName = groupName;
            
            // 座標パラメータの初期化
            X = new MetaDoubleParam(this, 0);
            Y = new MetaDoubleParam(this, 0);
            Scale = new MetaDoubleParam(this, 100);
            Alpha = new MetaDoubleParam(this, 0);
            Rotation = new MetaDoubleParam(this, 0);
        }

        [JsonConstructor]
        public GroupControlObject()
        {
            GroupName = string.Empty;
            X = new MetaDoubleParam(this, 0);
            Y = new MetaDoubleParam(this, 0);
            Scale = new MetaDoubleParam(this, 100);
            Alpha = new MetaDoubleParam(this, 0);
            Rotation = new MetaDoubleParam(this, 0);
        }

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
        {
            // グループ制御オブジェクト自体は描画しない（制御のみ）
            // 実際の描画処理はLayerObjectで行われる
        }

        /// <summary>
        /// 指定されたオブジェクトがこのグループ制御の影響範囲内にあるかどうかを判定
        /// </summary>
        /// <param name="targetObject">対象オブジェクト</param>
        /// <param name="frame">現在フレーム</param>
        /// <returns>影響範囲内の場合true</returns>
        public bool IsInEffectRange(MetasiaObject targetObject, int frame)
        {
            // グループ制御オブジェクト自体が有効でない場合
            if (!IsActive || !IsExistFromFrame(frame)) return false;

            // 対象オブジェクトとグループ制御の時間範囲の重複を計算
            int overlapStart = Math.Max(StartFrame, targetObject.StartFrame);
            int overlapEnd = Math.Min(EndFrame, targetObject.EndFrame);

            // 重複がある場合のみ制御対象
            return overlapStart <= overlapEnd && frame >= overlapStart && frame <= overlapEnd;
        }

        /// <summary>
        /// 指定されたフレームでの重複範囲を取得
        /// </summary>
        /// <param name="targetObject">対象オブジェクト</param>
        /// <returns>重複範囲の情報</returns>
        public OverlapRange GetOverlapRange(MetasiaObject targetObject)
        {
            int overlapStart = Math.Max(StartFrame, targetObject.StartFrame);
            int overlapEnd = Math.Min(EndFrame, targetObject.EndFrame);

            return new OverlapRange
            {
                StartFrame = overlapStart,
                EndFrame = overlapEnd,
                IsValid = overlapStart <= overlapEnd,
                GroupControlStart = StartFrame,
                GroupControlEnd = EndFrame,
                TargetObjectStart = targetObject.StartFrame,
                TargetObjectEnd = targetObject.EndFrame
            };
        }

        /// <summary>
        /// グループ変換を適用
        /// </summary>
        /// <param name="originalTransform">元の変換</param>
        /// <param name="frame">現在フレーム</param>
        /// <returns>適用後の変換</returns>
        public GroupTransform ApplyGroupTransform(GroupTransform originalTransform, int frame)
        {
            var groupTransform = new GroupTransform
            {
                X = X.Get(frame),
                Y = Y.Get(frame),
                Scale = Scale.Get(frame) / 100.0,
                Alpha = Alpha.Get(frame) / 100.0,
                Rotation = Rotation.Get(frame)
            };

            // エフェクトモードに応じて変換を適用
            switch (EffectMode)
            {
                case GroupEffectMode.Multiplicative:
                    return new GroupTransform
                    {
                        X = originalTransform.X + groupTransform.X,
                        Y = originalTransform.Y + groupTransform.Y,
                        Scale = originalTransform.Scale * groupTransform.Scale,
                        Alpha = originalTransform.Alpha + groupTransform.Alpha,
                        Rotation = originalTransform.Rotation + groupTransform.Rotation
                    };

                case GroupEffectMode.Additive:
                    return new GroupTransform
                    {
                        X = originalTransform.X + groupTransform.X,
                        Y = originalTransform.Y + groupTransform.Y,
                        Scale = originalTransform.Scale + (groupTransform.Scale - 1.0),
                        Alpha = originalTransform.Alpha + groupTransform.Alpha,
                        Rotation = originalTransform.Rotation + groupTransform.Rotation
                    };

                case GroupEffectMode.Override:
                    return groupTransform;

                default:
                    return originalTransform;
            }
        }
    }

    /// <summary>
    /// グループ変換パラメータ
    /// </summary>
    public struct GroupTransform
    {
        public double X;
        public double Y;
        public double Scale;
        public double Alpha;
        public double Rotation;

        public static GroupTransform Identity => new GroupTransform
        {
            X = 0,
            Y = 0,
            Scale = 1.0,
            Alpha = 0,
            Rotation = 0
        };
    }

    /// <summary>
    /// 重複範囲の情報
    /// </summary>
    public struct OverlapRange
    {
        public int StartFrame;
        public int EndFrame;
        public bool IsValid;
        public int GroupControlStart;
        public int GroupControlEnd;
        public int TargetObjectStart;
        public int TargetObjectEnd;

        /// <summary>
        /// 指定されたフレームが重複範囲内かどうか
        /// </summary>
        public bool ContainsFrame(int frame) => IsValid && frame >= StartFrame && frame <= EndFrame;

        /// <summary>
        /// 重複範囲の長さ
        /// </summary>
        public int Duration => IsValid ? EndFrame - StartFrame + 1 : 0;
    }

    /// <summary>
    /// グループ制御の合成モード
    /// </summary>
    public enum GroupBlendMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay,
        SoftLight,
        HardLight
    }

    /// <summary>
    /// グループエフェクト適用モード
    /// </summary>
    public enum GroupEffectMode
    {
        /// <summary>乗算的に適用（既存の値に掛け算）</summary>
        Multiplicative,
        /// <summary>加算的に適用（既存の値に足し算）</summary>
        Additive,
        /// <summary>上書き的に適用（既存の値を置き換え）</summary>
        Override
    }
} 