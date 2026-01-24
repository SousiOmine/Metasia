using System;
using System.Collections;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Editor.Controls;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using SoundIOSharp;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Models.States;
using System.Diagnostics;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media;
using Avalonia.Threading;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl, IDisposable
{
    private PlayerViewModel? VM
    {
        get { return this.DataContext as PlayerViewModel; }
    }

    private MediaAccessorRouter? mediaAccessorRouter;
    private readonly SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1, 1);
    private CancellationTokenSource _renderCts = new CancellationTokenSource();

    // フレームレート調整用のフィールド
    private int _pendingFrameRequest = -1;
    private bool _isRendering = false;

    public PlayerView()
    {
        InitializeComponent();

        this.DataContextChanged += (s, e) =>
        {
            try
            {
                var playbackState = App.Current?.Services?.GetRequiredService<IPlaybackState>();
                if (playbackState is not null)
                {
                    playbackState.ReRenderingRequested += RequestRender;
                    playbackState.PlaybackFrameChanged += RequestRender;
                }
                mediaAccessorRouter = App.Current?.Services?.GetRequiredService<MediaAccessorRouter>();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Failed to resolve IPlaybackState: {ex.Message}");
            }

            RequestRender();
        };
    }

    private void RequestRender()
    {
        // UIスレッド上での実行を保証
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RequestRender);
            return;
        }

        if (VM is null) return;

        var currentFrame = VM.Frame;

        // レンダリング中の場合は次のフレーム要求を記録
        if (_isRendering)
        {
            _pendingFrameRequest = currentFrame;
            return;
        }

        _ = HandlePaintAsync(currentFrame);
    }

    /// <summary>
    /// プレビュー表示領域に合わせたレンダリング解像度を計算する
    /// </summary>
    /// <param name="projectInfo">プロジェクト情報</param>
    /// <returns>レンダリング解像度</returns>
    private async Task<SKSize> GetPreviewRenderResolutionAsync(ProjectInfo projectInfo)
    {
        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // プレビューは表示領域に合わせた解像度でレンダリングし、
            // 低解像度の拡大表示によるジャギーを抑える。
            // ただし、プロジェクト解像度を超えても品質は上がらないので上限はプロジェクト解像度。

            var projectSize = projectInfo.Size;
            if (projectSize.Width <= 0 || projectSize.Height <= 0)
            {
                return new SKSize(960, 540);
            }

            var renderScaling = VisualRoot?.RenderScaling ?? 1.0;
            var controlWidthPx = skiaCanvas.Bounds.Width * renderScaling;
            var controlHeightPx = skiaCanvas.Bounds.Height * renderScaling;

            if (controlWidthPx <= 0 || controlHeightPx <= 0)
            {
                return new SKSize(960, 540);
            }

            var scale = Math.Min(controlWidthPx / projectSize.Width, controlHeightPx / projectSize.Height);
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
            {
                return new SKSize(960, 540);
            }

            scale = Math.Min(1.0, scale);

            var width = Math.Max(1, (int)Math.Round(projectSize.Width * scale));
            var height = Math.Max(1, (int)Math.Round(projectSize.Height * scale));

            return new SKSize(width, height);
        });
    }

    private async Task HandlePaintAsync(int requestedFrame)
    {
        if (VM is null || VM.TargetTimeline is null || VM.TargetProjectInfo is null || mediaAccessorRouter is null) return;

        _isRendering = true;

        try
        {
            await _renderSemaphore.WaitAsync();

            try
            {
                // 最新のフレーム要求を取得
                if (_pendingFrameRequest != -1)
                {
                    requestedFrame = _pendingFrameRequest;
                    _pendingFrameRequest = -1;
                }

                // 現在のフレームが変わっていなければレンダリング
                if (requestedFrame == VM.Frame)
                {
                    var compositor = new Compositor();
                    var projectInfo = VM.TargetProjectInfo;

                    var previewRenderResolution = await GetPreviewRenderResolutionAsync(projectInfo);

                    var bitmap = await compositor.RenderFrameAsync(
                        VM.TargetTimeline,
                        requestedFrame,
                        previewRenderResolution,
                        projectInfo.Size,
                        mediaAccessorRouter,
                        mediaAccessorRouter,
                        projectInfo,
                        VM.ProjectPath);

                    // フレームが変わっていなければビットマップを更新
                    if (requestedFrame == VM.Frame)
                    {
                        // GPU描画コントロールにビットマップを渡して再描画
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (requestedFrame == VM.Frame)
                            {
                                skiaCanvas.Bitmap = bitmap;
                                skiaCanvas.InvalidateSurface();
                            }
                            else
                            {
                                // UIスレッド到達時にフレームが変わっていた場合は破棄
                                bitmap?.Dispose();
                            }
                        });
                    }
                    else
                    {
                        bitmap?.Dispose();
                    }
                }

                // レンダリング中に新しい要求があれば再帰的に処理
                if (_pendingFrameRequest != -1)
                {
                    _ = HandlePaintAsync(_pendingFrameRequest);
                    _pendingFrameRequest = -1;
                }
            }
            finally
            {
                _renderSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during rendering: {ex.Message}");
        }
        finally
        {
            _isRendering = false;
        }
    }

    public void Dispose()
    {
        _renderCts?.Cancel();
        _renderCts?.Dispose();
        _renderSemaphore?.Dispose();
        // コントロールのリソースを解放
        skiaCanvas.Dispose();
    }
}
