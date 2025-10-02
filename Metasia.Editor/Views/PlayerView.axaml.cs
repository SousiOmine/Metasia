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
		canvas.Clear(SKColors.Green);

		lock (_bitmapLock)
		{
			if (_latestBitmap is not null)
			{
				canvas.DrawBitmap(_latestBitmap, 0, 0);
			}
		}

		if (_latestBitmap is null)
		{
			RequestRender();
		}
	}

	private void RequestRender()
	{
		_ = HandlePaintAsync();
	}

	private async Task HandlePaintAsync()
	{
		if (VM is null || VM.TargetTimeline is null || VM.TargetProjectInfo is null || mediaAccessorRouter is null) return;

		var previousCts = _renderCts;
		previousCts.Cancel();
		_renderCts = new CancellationTokenSource();
		previousCts.Dispose();

		var cancellationToken = _renderCts.Token;

		try
		{
			await _renderSemaphore.WaitAsync(cancellationToken);

			try
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				var compositor = new Compositor();
				var projectInfo = VM.TargetProjectInfo;
				var bitmap = await compositor.RenderFrameAsync(
					VM.TargetTimeline,
					VM.Frame,
					new SKSize(384, 216),
					new SKSize(3840, 2160),
					mediaAccessorRouter,
					mediaAccessorRouter,
					projectInfo,
					cancellationToken);

				if (cancellationToken.IsCancellationRequested)
				{
					bitmap?.Dispose();
					return;
				}

				lock (_bitmapLock)
				{
					_latestBitmap?.Dispose();
					_latestBitmap = bitmap;
				}

				Dispatcher.UIThread.Post(() =>
				{
					if (!cancellationToken.IsCancellationRequested)
					{
						skiaCanvas.InvalidateSurface();
					}
				});
			}
			finally
			{
				_renderSemaphore.Release();
			}
		}
		catch (OperationCanceledException)
		{
			Debug.WriteLine("Rendering was cancelled");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error during rendering: {ex.Message}");
			Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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