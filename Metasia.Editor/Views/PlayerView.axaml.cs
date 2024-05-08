using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Editor.Models;
using Metasia.Editor.ViewModels;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using SharpAudio;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	private PlayerViewModel? VM
	{
		get { return this.DataContext as PlayerViewModel; }
	}

	ProjectRenderer? renderer;

	private AudioEngine? audioEngine;
	private AudioBuffer? audioBuffer;
	private AudioSource? audioSource;
	private AudioFormat audioFormat;


    public PlayerView()
    {
        InitializeComponent();
		
		this.DataContextChanged += (s, e) =>
		{
			if (VM is not null) VM.ViewPaintRequest = () => { skiaCanvas.InvalidateSurface(); };
		};
		audioEngine = AudioEngine.CreateDefault();
		if (audioEngine is not null)
		{
			audioBuffer = audioEngine.CreateBuffer();
			audioSource = audioEngine.CreateSource();
			
		}
		audioFormat = new AudioFormat();
		audioFormat.BitsPerSample = 16;
		audioFormat.Channels = 2;
		audioFormat.SampleRate = 44100;
		
    }

	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		if (VM is null) return;
		if (renderer is null)
		{
			if (MetasiaProvider.MetasiaProject is null) return;
			else renderer = new ProjectRenderer(MetasiaProvider.MetasiaProject);
		}
		SKImageInfo info = e.Info;
		SKSurface surface = e.Surface;
		SKCanvas canvas = surface.Canvas;
		canvas.Clear(SKColors.Green);

		
		ExpresserArgs exp = new()
		{
			bitmap = new SKBitmap(384, 216),
			sound = MetasiaSound.CreateMetasiaSound(new MetasiaSoundOption(){Channel = 2}),
			targetSize = new SKSize(3840, 2160),
			ResolutionLevel = 0.1f,
			SoundChannel = 2,
			SoundSampleRate = 44100,
			Fps = (ushort)MetasiaProvider.MetasiaProject.Info.Framerate
		};
		renderer.Render(ref exp, VM.Frame);
		canvas.DrawBitmap(exp.bitmap, 0, 0);

		if (audioEngine is not null)
		{
			audioBuffer.BufferData(exp.sound.Pulse, audioFormat);
			audioSource.QueueBuffer(audioBuffer);
			audioSource.Play();
		}
		
		exp.Dispose();


	}
	
}