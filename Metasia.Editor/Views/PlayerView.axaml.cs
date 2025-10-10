using System;
using System.Collections;
using System.Collections.Concurrent;
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

    private MediaAccessorRouter mediaAccessorRouter;
    private readonly SemaphoreSlim _renderSemaphore = new SemaphoreSlim(1, 1);
    private CancellationTokenSource _renderCts = new CancellationTokenSource();
    private readonly object _bitmapLock = new object();
    private SKBitmap? _latestBitmap;

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

    private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
    {
        SKSurface surface = e.Surface;
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        lock (_bitmapLock)
        {
            if (_latestBitmap is not null)
            {
                // キャンバスの中央にビットマップを描画
                var canvasWidth = e.Info.Width;
                var canvasHeight = e.Info.Height;
                var bitmapWidth = _latestBitmap.Width;
                var bitmapHeight = _latestBitmap.Height;

                // アスペクト比を維持したままキャンバスにフィットさせる
                var scaleX = (float)canvasWidth / bitmapWidth;
                var scaleY = (float)canvasHeight / bitmapHeight;
                var scale = Math.Min(scaleX, scaleY);

                var scaledWidth = bitmapWidth * scale;
                var scaledHeight = bitmapHeight * scale;

                var x = (canvasWidth - scaledWidth) / 2;
                var y = (canvasHeight - scaledHeight) / 2;

                var destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);
                canvas.DrawBitmap(_latestBitmap, destRect);
            }
        }

        if (_latestBitmap is null)
        {
            RequestRender();
        }
    }

    private void RequestRender()
    {
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

                    var bitmap = await compositor.RenderFrameAsync(
                        VM.TargetTimeline,
                        requestedFrame,
                        new SKSize(960, 540),
                        new SKSize(3840, 2160),
                        mediaAccessorRouter,
                        mediaAccessorRouter,
                        projectInfo);

                    // フレームが変わっていなければビットマップを更新
                    if (requestedFrame == VM.Frame)
                    {
                        lock (_bitmapLock)
                        {
                            _latestBitmap?.Dispose();
                            _latestBitmap = bitmap;
                        }

                        Dispatcher.UIThread.Post(() =>
                        {
                            if (requestedFrame == VM.Frame)
                            {
                                skiaCanvas.InvalidateSurface();
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
        lock (_bitmapLock)
        {
            _latestBitmap?.Dispose();
            _latestBitmap = null;
        }
    }
}