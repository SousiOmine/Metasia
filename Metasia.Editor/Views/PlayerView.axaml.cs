using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	private PlayerViewModel? VM
	{
		get { return this.DataContext as PlayerViewModel; }
	}

	object renderLock = new object();

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
		if (VM is null || VM.TargetTimeline is null) return;

		var compositor = new Compositor();
		var bitmap = compositor.RenderFrame(VM.TargetTimeline, VM.Frame, new SKSize(384, 216), new SKSize(3840, 2160));

		lock (renderLock)
		{
			SKImageInfo info = e.Info;
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;
			canvas.Clear(SKColors.Green);

			canvas.DrawBitmap(bitmap, 0, 0);

			bitmap.Dispose();
		}

	}

}