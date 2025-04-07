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

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	private PlayerViewModel? VM
	{
		get { return this.DataContext as PlayerViewModel; }
	}


	object renderLock = new object();

	private IAudioService audioService;

	public PlayerView()
    {
        InitializeComponent();
		
		this.DataContextChanged += (s, e) =>
		{
			if (VM is not null) VM.ViewPaintRequest = () => { skiaCanvas.InvalidateSurface(); };
			if (VM is not null) VM.PlayStart = PlayStart;

			skiaCanvas.InvalidateSurface();
		};
		
		audioService = new SoundIOService();
    }
	
	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		if (VM is null) return;
		//if (VM.TargetTimeline is null) return;

		DrawExpresserArgs drawExp = new()
		{
			Bitmap = new SKBitmap(384, 216),
			ActualResolution = new SKSize(384, 216),
			TargetResolution = new SKSize(3840, 2160),
			FPS = VM.TargetProjectInfo.Framerate,
		};
		
		using (SKCanvas canvas = new SKCanvas(drawExp.Bitmap))
		{
			canvas.Clear(SKColors.Black);
		}
		
		VM.TargetTimeline.DrawExpresser(ref drawExp, VM.Frame);

		if(VM.IsPlaying)
		{
            AudioExpresserArgs audioExp = new()
            {
                Sound = new MetasiaSound(2, 44100, 60),
                AudioChannel = 2,
                SoundSampleRate = 44100,
                FPS = VM.TargetProjectInfo.Framerate,
            };

            VM.TargetTimeline.AudioExpresser(ref audioExp, VM.Frame);


            audioService.InsertQueue(audioExp.Sound.Pulse, 2);

        }



		lock (renderLock)
		{
			
			SKImageInfo info = e.Info;
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;
			canvas.Clear(SKColors.Green);

			canvas.DrawBitmap(drawExp.Bitmap, 0, 0);
		}

	}

	private void PlayStart()
	{
		double[] pulse = new double[8820];
		audioService.InsertQueue(pulse, 2);
	}

}