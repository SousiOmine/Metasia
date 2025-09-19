using Metasia.Core.Media;
using Metasia.Core.Objects;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// IRenderableなオブジェクトから生成されたレンダーツリーを解析し、最終的なビットマップに合成する
    /// </summary>
    public class Compositor
    {
        /// <summary>
        /// 指定フレームの最終的なビットマップを生成する
        /// </summary>
        /// <param name="root">描画ツリーの√となるオブジェクト(TimelineObjectなど)</param>
        /// <param name="frame">描画するフレームの番号</param>
        /// <param name="renderResolution">レンダリング解像度</param>
        /// <param name="projectResolution">プロジェクトの解像度</param>
        /// <returns>合成後のビットマップ</returns>
        public SKBitmap RenderFrame(
            IRenderable root,
            int frame,
            SKSize renderResolution,
            SKSize projectResolution,
            IImageFileAccessor imageFileAccessor,
            IVideoFileAccessor videoFileAccessor)
        {
            ArgumentNullException.ThrowIfNull(root);
            if (renderResolution.Width <= 0 || renderResolution.Height <= 0) throw new ArgumentOutOfRangeException(nameof(renderResolution), "Render resolution must be positive");
            if (projectResolution.Width <= 0 || projectResolution.Height <= 0) throw new ArgumentOutOfRangeException(nameof(projectResolution), "Project resolution must be positive");
            
            var resultBitmap = new SKBitmap((int)renderResolution.Width, (int)renderResolution.Height);
            using (SKCanvas canvas = new SKCanvas(resultBitmap))
            {
                //下地は黒で塗りつぶす
			    canvas.Clear(SKColors.Black);

                var context = new RenderContext(frame, projectResolution, renderResolution, imageFileAccessor, videoFileAccessor);
                var rootNode = root.Render(context);

                ProcessNode(canvas, rootNode, projectResolution, renderResolution);
            }
            
            return resultBitmap;
        }

        private void ProcessNode(SKCanvas canvas, RenderNode node, SKSize projectResolution, SKSize renderResolution)
        {
            // 子ノードを再帰的に描画
            foreach (var child in node.Children)
            {
                ProcessNode(canvas, child, projectResolution, renderResolution);
            }

            if (node.Bitmap is not null)
            {
                canvas.Save();

                float renderScaleWidth = renderResolution.Width / projectResolution.Width;
                float renderScaleHeight = renderResolution.Height / projectResolution.Height;

                // 論理的なスケールとレンダリングスケールを組み合わせる
                float totalScaleX = node.Transform.Scale * renderScaleWidth;
                float totalScaleY = node.Transform.Scale * renderScaleHeight;

                // LogicalSize を基準に、最終的な描画矩形（サイズと位置）を計算
                float finalWidth = node.LogicalSize.Width * totalScaleX;
                float finalHeight = node.LogicalSize.Height * totalScaleY;
                
                var canvasSize = canvas.DeviceClipBounds;
                float finalX = (node.Transform.Position.X * renderScaleWidth) + (canvasSize.Width / 2f) - (finalWidth / 2f);
                float finalY = (-node.Transform.Position.Y * renderScaleHeight) + (canvasSize.Height / 2f) - (finalHeight / 2f);
                
                var destRect = SKRect.Create(finalX, finalY, finalWidth, finalHeight);

                // Transformの回転と不透明度を適用
                canvas.RotateDegrees(node.Transform.Rotation, destRect.MidX, destRect.MidY);
                using (var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(node.Transform.Alpha * 255)) })
                {
                    // DrawBitmapで、Bitmapを指定した矩形(destRect)に描画する
                    canvas.DrawBitmap(node.Bitmap, destRect, paint);
                }
                canvas.Restore();
            }
        }
    }
}