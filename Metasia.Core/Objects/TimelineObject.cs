using Metasia.Core.Graphics;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Sounds;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Metasia.Core.Objects
{
	/// <summary>
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : MetasiaObject, IMetaDrawable, IMetaAudiable
	{
		/// <summary>
		/// タイムラインに属するレイヤー 格納順に描画される
		/// </summary>
		[JsonInclude]
		public List<LayerObject> Layers { get; private set; }

		public double Volume { get; set; } = 100;

        public TimelineObject(string id) : base(id)
		{
			Layers = new();
		}

		public TimelineObject()
        {
			Layers = new();
        }

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
			double resolution_level_x = e.ActualResolution.Width / e.TargetResolution.Width;
			double resolution_level_y = e.ActualResolution.Height / e.TargetResolution.Height;
			
			//DrawExpresserArgsのSKBitmapのインスタンスがなかったら生成
			if (e.Bitmap is null) e.Bitmap = new SKBitmap((int)(e.ActualResolution.Width), (int)(e.ActualResolution.Height));

            // グループ制御オブジェクトを収集
            var groupControls = GetActiveGroupControls(frame);

            for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
			{
                var layer = Layers[layerIndex];
				if (!layer.IsActive) continue;

                DrawExpresserArgs express = new()
                {
	                ActualResolution = e.ActualResolution,
                    TargetResolution = e.TargetResolution,
                    FPS = e.FPS
                };

                // レイヤーに影響するグループ制御を特定
                var affectingGroupControls = GetAffectingGroupControls(groupControls, layerIndex, frame);

                // グループ制御を考慮してレイヤーを描画
                DrawLayerWithGroupControl(ref express, layer, affectingGroupControls, frame);
				
                if (express.Bitmap is null) 
                {
                    express.Dispose();
                    continue;
                }

                using (SKCanvas canvas = new SKCanvas(e.Bitmap))
				{
					canvas.DrawBitmap(express.Bitmap, 0, 0);
				}

				express.Dispose();
            }
            
            e.ActualSize = new SKSize(e.Bitmap.Width, e.Bitmap.Height);
			e.TargetSize = e.TargetResolution;
		}
		

		public void AudioExpresser(ref AudioExpresserArgs e, int frame)
		{
			//AudioExpresserArgsのMetasiaSoundのインスタンスがなかったら生成
			if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);

			foreach (var layer in Layers)
			{
                if (!layer.IsActive) continue;
                AudioExpresserArgs express = new()
                {
                    AudioChannel = e.AudioChannel,
                    SoundSampleRate = e.SoundSampleRate,
                    FPS = e.FPS
                };
				layer.AudioExpresser(ref express, frame);

				if(express.Sound is null) continue;

                if (layer.Volume != 100)
                {
                    express.Sound = MetasiaSound.VolumeChange(express.Sound, layer.Volume / 100);
                }

                e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);

				express.Dispose();
            }
			
		}

        /// <summary>
        /// 指定されたフレームで有効なグループ制御オブジェクトを取得
        /// </summary>
        private List<(GroupControlObject GroupControl, int LayerIndex)> GetActiveGroupControls(int frame)
        {
            var result = new List<(GroupControlObject, int)>();

            for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
            {
                var layer = Layers[layerIndex];
                if (!layer.IsActive) continue;

                foreach (var obj in layer.Objects)
                {
                    if (obj is GroupControlObject groupControl && 
                        groupControl.IsActive && 
                        groupControl.IsExistFromFrame(frame))
                    {
                        result.Add((groupControl, layerIndex));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 指定されたレイヤーに影響するグループ制御オブジェクトを取得
        /// </summary>
        private List<GroupControlObject> GetAffectingGroupControls(
            List<(GroupControlObject GroupControl, int LayerIndex)> allGroupControls, 
            int targetLayerIndex, 
            int frame)
        {
            var result = new List<GroupControlObject>();

            foreach (var (groupControl, groupLayerIndex) in allGroupControls)
            {
                // グループ制御オブジェクトより上位のレイヤーかつ、制御範囲内のレイヤーの場合
                if (groupLayerIndex < targetLayerIndex && 
                    (targetLayerIndex - groupLayerIndex) <= groupControl.TargetLayerDepth)
                {
                    result.Add(groupControl);
                }
            }

            return result;
        }

        /// <summary>
        /// グループ制御を適用してレイヤーを描画
        /// </summary>
        private void DrawLayerWithGroupControl(ref DrawExpresserArgs e, LayerObject layer, 
            List<GroupControlObject> groupControls, int frame)
        {
            if (groupControls.Count == 0)
            {
                // グループ制御がない場合は通常の描画
                layer.DrawExpresser(ref e, frame);
                return;
            }

            // レイヤー内にグループ制御の影響を受けるオブジェクトがあるかチェック
            bool hasAffectedObjects = false;
            foreach (var obj in layer.Objects)
            {
                if (obj.IsExistFromFrame(frame) && obj is IMetaDrawable && obj.IsActive)
                {
                    var affectingControls = groupControls.Where(gc => gc.IsInEffectRange(obj, frame));
                    if (affectingControls.Any())
                    {
                        hasAffectedObjects = true;
                        break;
                    }
                }
            }

            if (!hasAffectedObjects)
            {
                // 影響を受けるオブジェクトがない場合は通常の描画
                layer.DrawExpresser(ref e, frame);
                return;
            }

            // レイヤー全体を描画
            DrawExpresserArgs layerExpress = new()
            {
                ActualResolution = e.ActualResolution,
                TargetResolution = e.TargetResolution,
                FPS = e.FPS
            };

            layer.DrawExpresser(ref layerExpress, frame);

            if (layerExpress.Bitmap == null)
            {
                layerExpress.Dispose();
                return;
            }

            // レイヤー全体に対してグループ制御の変換を計算
            var layerTransform = GroupTransform.Identity;

            // レイヤー内のオブジェクトから影響するグループ制御を判定
            foreach (var obj in layer.Objects)
            {
                if (obj.IsExistFromFrame(frame) && obj is IMetaDrawable && obj.IsActive)
                {
                    var affectingControls = groupControls.Where(gc => gc.IsInEffectRange(obj, frame)).ToList();
                    
                    // 影響するグループ制御の変換を累積的に適用
                    foreach (var groupControl in affectingControls)
                    {
                        layerTransform = groupControl.ApplyGroupTransform(layerTransform, frame);
                    }
                    
                    // 最初に影響を受けるオブジェクトの変換を適用（レイヤー全体の代表として）
                    break;
                }
            }

            // レイヤー全体に変換を適用
            if (e.Bitmap == null)
            {
                e.Bitmap = new SKBitmap((int)e.ActualResolution.Width, (int)e.ActualResolution.Height);
            }

            ApplyLayerTransformAndDraw(ref e, ref layerExpress, layerTransform);

            layerExpress.Dispose();

            e.ActualSize = new SKSize(e.Bitmap.Width, e.Bitmap.Height);
            e.TargetSize = e.TargetResolution;
        }

        /// <summary>
        /// オブジェクトの基本変換を取得
        /// </summary>
        private GroupTransform GetObjectTransform(MetasiaObject obj, int frame)
        {
            if (obj is IMetaCoordable coordObj)
            {
                return new GroupTransform
                {
                    X = coordObj.X.Get(frame),
                    Y = coordObj.Y.Get(frame),
                    Scale = coordObj.Scale.Get(frame) / 100.0,
                    Alpha = coordObj.Alpha.Get(frame) / 100.0,
                    Rotation = coordObj.Rotation.Get(frame)
                };
            }

            return GroupTransform.Identity;
        }

        /// <summary>
        /// 変換を適用してオブジェクトを描画
        /// </summary>
        private void ApplyTransformAndDraw(ref DrawExpresserArgs mainExpress, ref DrawExpresserArgs objExpress, GroupTransform transform)
        {
            var bitmap = objExpress.Bitmap;

            // 回転の適用
            if (transform.Rotation != 0)
            {
                bitmap = MetasiaBitmap.Rotate(bitmap, transform.Rotation);
            }

            // 透明度の適用
            if (transform.Alpha != 0.0)
            {
                double alphaMultiplier = Math.Max(0, Math.Min(1, 1.0 - transform.Alpha));
                bitmap = MetasiaBitmap.Transparency(bitmap, alphaMultiplier);
            }

            // スケールと位置の計算
            double width = objExpress.TargetSize?.Width ?? bitmap.Width;
            double height = objExpress.TargetSize?.Height ?? bitmap.Height;
            
            width *= transform.Scale;
            height *= transform.Scale;

            SKRect drawPos = new SKRect()
            {
                Left = (float)((mainExpress.TargetResolution.Width - width) / 2 + transform.X) * 
                       (mainExpress.ActualResolution.Width / mainExpress.TargetResolution.Width),
                Top = (float)((mainExpress.TargetResolution.Height - height) / 2 - transform.Y) * 
                      (mainExpress.ActualResolution.Height / mainExpress.TargetResolution.Height),
                Right = (float)(((mainExpress.TargetResolution.Width - width) / 2 + transform.X + width) * 
                               (mainExpress.ActualResolution.Width / mainExpress.TargetResolution.Width)),
                Bottom = (float)(((mainExpress.TargetResolution.Height - height) / 2 - transform.Y + height) * 
                                (mainExpress.ActualResolution.Height / mainExpress.TargetResolution.Height))
            };

            using (SKCanvas canvas = new SKCanvas(mainExpress.Bitmap))
            {
                canvas.DrawBitmap(bitmap, drawPos);
            }

            // 作成したビットマップを破棄
            if (bitmap != objExpress.Bitmap)
            {
                bitmap.Dispose();
            }
        }

        /// <summary>
        /// レイヤー全体に変換を適用して描画
        /// </summary>
        private void ApplyLayerTransformAndDraw(ref DrawExpresserArgs mainExpress, ref DrawExpresserArgs layerExpress, GroupTransform transform)
        {
            var bitmap = layerExpress.Bitmap;

            // 回転の適用
            if (transform.Rotation != 0)
            {
                bitmap = MetasiaBitmap.Rotate(bitmap, transform.Rotation);
            }

            // 透明度の適用
            if (transform.Alpha != 0.0)
            {
                double alphaMultiplier = Math.Max(0, Math.Min(1, 1.0 - transform.Alpha));
                bitmap = MetasiaBitmap.Transparency(bitmap, alphaMultiplier);
            }

            // レイヤー全体のサイズを取得（レイヤーはプロジェクトサイズと同じ）
            double width = mainExpress.TargetResolution.Width;
            double height = mainExpress.TargetResolution.Height;
            
            // スケールを適用
            width *= transform.Scale;
            height *= transform.Scale;

            // 描画位置を計算（レイヤー全体の中央を基準）
            SKRect drawPos = new SKRect()
            {
                Left = (float)((mainExpress.TargetResolution.Width - width) / 2 + transform.X) * 
                       (mainExpress.ActualResolution.Width / mainExpress.TargetResolution.Width),
                Top = (float)((mainExpress.TargetResolution.Height - height) / 2 - transform.Y) * 
                      (mainExpress.ActualResolution.Height / mainExpress.TargetResolution.Height),
                Right = (float)(((mainExpress.TargetResolution.Width - width) / 2 + transform.X + width) * 
                               (mainExpress.ActualResolution.Width / mainExpress.TargetResolution.Width)),
                Bottom = (float)(((mainExpress.TargetResolution.Height - height) / 2 - transform.Y + height) * 
                                (mainExpress.ActualResolution.Height / mainExpress.TargetResolution.Height))
            };

            using (SKCanvas canvas = new SKCanvas(mainExpress.Bitmap))
            {
                canvas.DrawBitmap(bitmap, drawPos);
            }

            // 作成したビットマップを破棄
            if (bitmap != layerExpress.Bitmap)
            {
                bitmap.Dispose();
            }
        }
	}
}
