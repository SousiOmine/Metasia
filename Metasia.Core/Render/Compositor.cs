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
        /// <returns>合成後のイメージ</returns>
        public async Task<SKImage> RenderFrameAsync(
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

            var info = new SKImageInfo((int)renderResolution.Width, (int)renderResolution.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            try
            {
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
                throw;
            }

            return surface.Snapshot();
        }

        private async Task ProcessNodeAsync(SKCanvas canvas, IRenderNode node, SKSize projectResolution, SKSize renderResolution, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (node)
            {
                case NormalRenderNode normalNode:
                    // 通常のノード処理
                    await ProcessNormalNodeAsync(canvas, normalNode, projectResolution, renderResolution, cancellationToken);
                    break;
                default:
                    // 未知のノードタイプ
                    break;
            }

            
        }

        private async Task ProcessNormalNodeAsync(SKCanvas canvas, NormalRenderNode node, SKSize projectResolution, SKSize renderResolution, CancellationToken cancellationToken = default)
        {
            // 子ノードを再帰的に描画
            foreach (var child in node.Children)
            {
                await ProcessNodeAsync(canvas, child, projectResolution, renderResolution, cancellationToken);
            }

            if (node.Image is not null)
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
                var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
                using (var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(node.Transform.Alpha * 255)), IsAntialias = true })
                {
                    try
                    {
                        if (node.Image is not null && node.Image.Width > 0 && node.Image.Height > 0 && !cancellationToken.IsCancellationRequested)
                        {
                            canvas.DrawImage(node.Image, destRect, sampling, paint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to draw image: {ex.Message}");
                    }
                }
                canvas.Restore();
            }
        }
    }
}
