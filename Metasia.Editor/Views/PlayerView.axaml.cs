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
using Metasia.Editor.Models;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using SoundIOSharp;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	private PlayerViewModel? VM
	{
		get { return this.DataContext as PlayerViewModel; }
	}

	ProjectRenderer? renderer;

	object renderLock = new object();

	private IAudioService audioService;

	public PlayerView()
    {
        InitializeComponent();
		
		this.DataContextChanged += (s, e) =>
		{
			if (VM is not null) VM.ViewPaintRequest = () => { skiaCanvas.InvalidateSurface(); };
			if (VM is not null) VM.PlayStart = PlayStart;
		};
		
		audioService = new SoundIOService();
    }
	
	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		if (VM is null) return;
		if (renderer is null)
		{
			if (MetasiaProvider.MetasiaProject is null) return;
			else renderer = new ProjectRenderer(MetasiaProvider.MetasiaProject);
		}
		ExpresserArgs exp = new()
		{
			bitmap = new SKBitmap(384, 216),
			sound = new MetasiaSound(2, 44100, 60),
			targetSize = new SKSize(3840, 2160),
			ResolutionLevel = 0.1f,
			AudioChannel = 2
		};
		renderer.Render(ref exp, VM.Frame);

		audioService.InsertQueue(exp.sound.Pulse, 2);

		lock (renderLock)
		{
			
			SKImageInfo info = e.Info;
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;
			canvas.Clear(SKColors.Green);


			
			//Console.WriteLine(soundQueue.Count);
			
			
			
			canvas.DrawBitmap(exp.bitmap, 0, 0);
		}

	}

	private void PlayStart()
	{
		double[] pulse = new double[8820];
		audioService.InsertQueue(pulse, 2);
	}

}