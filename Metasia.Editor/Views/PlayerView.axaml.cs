using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Avalonia.Threading;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl, IDisposable
{
    private const int NoPendingFrame = -1;

    private PlayerViewModel? VM
    {
        get { return this.DataContext as PlayerViewModel; }
    }

    private MediaAccessorRouter? mediaAccessorRouter;
    private readonly SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1, 1);
    private int _pendingFrameRequest = NoPendingFrame;
    private bool _isRendering = false;
    private bool _disposed = false;
    private IPlaybackState? _currentPlaybackState;

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
                    if (_currentPlaybackState is not null)
                    {
                        _currentPlaybackState.ReRenderingRequested -= RequestRender;
                        _currentPlaybackState.PlaybackFrameChanged -= RequestRender;
                    }

                    playbackState.ReRenderingRequested += RequestRender;
                    playbackState.PlaybackFrameChanged += RequestRender;
                    _currentPlaybackState = playbackState;
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
        if (_disposed)
        {
            return;
        }

        // UIスレッド上での実行を保証
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RequestRender);
            return;
        }

        if (VM is null) return;

        var currentFrame = VM.Frame;

        // レンダリング中は「最後に要求されたフレーム」だけを保持する
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
        if (_disposed)
        {
            return;
        }

        _isRendering = true;

        try
        {
            await _renderSemaphore.WaitAsync();

            try
            {
                while (true)
                {
                    if (VM is null || VM.TargetTimeline is null || VM.TargetProjectInfo is null || mediaAccessorRouter is null)
                    {
                        return;
                    }

                    // レンダリング待ちがあれば常に最新要求へ追従
                    if (_pendingFrameRequest != NoPendingFrame)
                    {
                        requestedFrame = _pendingFrameRequest;
                        _pendingFrameRequest = NoPendingFrame;
                    }

                    var compositor = new Compositor();
                    var projectInfo = VM.TargetProjectInfo;
                    var previewRenderResolution = await GetPreviewRenderResolutionAsync(projectInfo);

                    var image = await compositor.RenderFrameAsync(
                        VM.TargetTimeline,
                        requestedFrame,
                        previewRenderResolution,
                        projectInfo.Size,
                        mediaAccessorRouter,
                        mediaAccessorRouter,
                        projectInfo,
                        VM.ProjectPath);

                    // 「最新完了フレーム」を表示する
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_disposed)
                        {
                            image.Dispose();
                            return;
                        }

                        skiaCanvas.Image = image;
                        skiaCanvas.InvalidateSurface();
                    });

                    if (_pendingFrameRequest == NoPendingFrame)
                    {
                        break;
                    }
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_currentPlaybackState is not null)
        {
            _currentPlaybackState.ReRenderingRequested -= RequestRender;
            _currentPlaybackState.PlaybackFrameChanged -= RequestRender;
            _currentPlaybackState = null;
        }

        _renderSemaphore.Dispose();
        skiaCanvas.Dispose();
    }
}
