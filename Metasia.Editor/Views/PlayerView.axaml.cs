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
	private readonly object _renderLock = new object();

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
					playbackState.ReRenderingRequested += () => { skiaCanvas.InvalidateSurface(); };
					playbackState.PlaybackFrameChanged += () => { skiaCanvas.InvalidateSurface(); };
				}
				mediaAccessorRouter = App.Current?.Services?.GetRequiredService<MediaAccessorRouter>();
			}
			catch (InvalidOperationException ex)
			{
				Debug.WriteLine($"Failed to resolve IPlaybackState: {ex.Message}");
			}

			skiaCanvas.InvalidateSurface();
		};    
	}
	
	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		_ = HandlePaintAsync(e);
	}

	private async Task HandlePaintAsync(Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		if (VM is null || VM.TargetTimeline is null) return;

		// 前のレンダリングをキャンセル
		_renderCts.Cancel();
		_renderCts.Dispose();
		_renderCts = new CancellationTokenSource();

		try
		{
			// 排他制御で並行レンダリングを防止
			await _renderSemaphore.WaitAsync(_renderCts.Token);

			try
			{
				// キャンセルトークンをチェック
				if (_renderCts.Token.IsCancellationRequested)
					return;

				var compositor = new Compositor();
				var bitmap = await compositor.RenderFrameAsync(
					VM.TargetTimeline,
					VM.Frame,
					new SKSize(384, 216),
					new SKSize(3840, 2160),
					mediaAccessorRouter,
					mediaAccessorRouter,
					_renderCts.Token);

				// キャンセルチェック
				if (_renderCts.Token.IsCancellationRequested)
				{
					bitmap?.Dispose();
					return;
				}

				lock (_renderLock)
				{
					// 最終的なキャンセルチェック
					if (_renderCts.Token.IsCancellationRequested)
					{
						bitmap?.Dispose();
						return;
					}

					SKImageInfo info = e.Info;
					SKSurface surface = e.Surface;
					SKCanvas canvas = surface.Canvas;
					canvas.Clear(SKColors.Green);

					if (bitmap != null)
					{
						canvas.DrawBitmap(bitmap, 0, 0);
						bitmap.Dispose();
					}
				}
			}
			finally
			{
				_renderSemaphore.Release();
			}
		}
		catch (OperationCanceledException)
		{
			// キャンセルされた場合は無視
			Debug.WriteLine("Rendering was cancelled");
		}
		catch (Exception ex)
		{
			// その他の例外をログに記録
			Debug.WriteLine($"Error during rendering: {ex.Message}");
			Debug.WriteLine($"Stack trace: {ex.StackTrace}");
		}
	}

	public void Dispose()
	{
		_renderCts?.Cancel();
		_renderCts?.Dispose();
		_renderSemaphore?.Dispose();
	}
}