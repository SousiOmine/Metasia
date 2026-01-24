using System.Diagnostics;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// IRenderableなオブジェクトから生成されたレンダーツリーを解析し、最終的なビットマップに合成する
    /// </summary>
    public class Compositor
    {
        /// <summary>
        /// 指定フレームの最終的なビットマップを非同期に生成する
        /// </summary>
        /// <param name="root">描画ツリーの√となるオブジェクト(TimelineObjectなど)</param>
        /// <param name="frame">描画するフレームの番号</param>
        /// <param name="renderResolution">レンダリング解像度</param>
        /// <param name="projectResolution">プロジェクトの解像度</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>合成後のビットマップ</returns>
        public async Task<SKBitmap> RenderFrameAsync(
            IRenderable root,
            int frame,
            SKSize renderResolution,
            SKSize projectResolution,
            IImageFileAccessor imageFileAccessor,
            IVideoFileAccessor videoFileAccessor,
            ProjectInfo projectInfo,
            string projectPath,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(root);
            if (renderResolution.Width <= 0 || renderResolution.Height <= 0) throw new ArgumentOutOfRangeException(nameof(renderResolution), "Render resolution must be positive");
            if (projectResolution.Width <= 0 || projectResolution.Height <= 0) throw new ArgumentOutOfRangeException(nameof(projectResolution), "Project resolution must be positive");

            cancellationToken.ThrowIfCancellationRequested();

            var resultBitmap = new SKBitmap((int)renderResolution.Width, (int)renderResolution.Height);
            try
            {
                using SKCanvas canvas = new(resultBitmap);
                //下地は黒で塗りつぶす
                canvas.Clear(SKColors.Black);

                cancellationToken.ThrowIfCancellationRequested();

                var context = new RenderContext(frame, projectResolution, renderResolution, imageFileAccessor, videoFileAccessor, projectInfo, projectPath);
                var rootNode = await root.RenderAsync(context, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await ProcessNodeAsync(canvas, rootNode, projectResolution, renderResolution, cancellationToken);
            }
            catch
            {
                resultBitmap.Dispose();
                throw;
            }

            return resultBitmap;
        }

        private async Task ProcessNodeAsync(SKCanvas canvas, RenderNode node, SKSize projectResolution, SKSize renderResolution, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 子ノードを再帰的に描画
            foreach (var child in node.Children)
            {
                await ProcessNodeAsync(canvas, child, projectResolution, renderResolution, cancellationToken);
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
                using (var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(node.Transform.Alpha * 255)), IsAntialias = true })
                {
                    try
                    {
                        if (node.Bitmap is not null && node.Bitmap.Width > 0 && node.Bitmap.Height > 0 && !cancellationToken.IsCancellationRequested)
                        {
                            // DrawBitmapで、Bitmapを指定した矩形(destRect)に描画する
                            canvas.DrawBitmap(node.Bitmap, destRect, paint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to draw bitmap: {ex.Message}");
                    }
                }
                canvas.Restore();
            }
        }
    }
}