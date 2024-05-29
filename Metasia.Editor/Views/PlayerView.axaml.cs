using System;
using System.Collections;
using System.Collections.Generic;
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
using SoundIOSharp;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	private static Action<IntPtr, double>? write_sample;
	static Queue<double> soundQueue = new Queue<double>();
	private static SoundIO soundIo;
	static SoundIODevice device;
	static SoundIOOutStream outStream;
	
	
	private PlayerViewModel? VM
	{
		get { return this.DataContext as PlayerViewModel; }
	}

	ProjectRenderer? renderer;

	object renderLock = new object();


	public PlayerView()
    {
        InitializeComponent();
		
		this.DataContextChanged += (s, e) =>
		{
			if (VM is not null) VM.ViewPaintRequest = () => { skiaCanvas.InvalidateSurface(); };
		};
		
		soundIo = new SoundIO();
		soundIo.Connect();
		soundIo.FlushEvents();

		device = soundIo.GetOutputDevice(soundIo.DefaultOutputDeviceIndex);
		
		outStream = device.CreateOutStream();
		
		outStream.WriteCallback = (min, max) => write_callback(outStream, min, max);
		
		outStream.SampleRate = 44100;
		
		if (device.SupportsFormat(SoundIODevice.Float32FE))
		{
			outStream.Format = SoundIODevice.Float32FE;
			write_sample = write_sample_float32ne;
		}
		else if (device.SupportsFormat(SoundIODevice.Float64NE))
		{
			outStream.Format = SoundIODevice.Float64NE;
			write_sample = write_sample_float64ne;
		}
		else if (device.SupportsFormat(SoundIODevice.S32NE))
		{
			outStream.Format = SoundIODevice.S32NE;
			write_sample = write_sample_s32ne;
		}
		else if (device.SupportsFormat(SoundIODevice.S16NE))
		{
			outStream.Format = SoundIODevice.S16NE;
			write_sample = write_sample_s16ne;
		}
		else
		{
			Console.Error.WriteLine("No suitable format available.");
			return;
		}
		
		outStream.Open();
		outStream.Start();
		
    }
	
	private static void write_callback(SoundIOOutStream outstream, int frame_count_min, int frame_count_max)
	{
		double float_sample_rate = outstream.SampleRate;
		double seconds_per_frame = 1.0 / float_sample_rate;
		
		int frames_left = frame_count_max;
		
		if (frame_count_max > 1470)
		{
			frames_left = 1470;
		}
			
		int frame_count = 0;

		for (;;)
		{
			frame_count = frames_left;
			var results = outstream.BeginWrite(ref frame_count);
			
			if (frame_count == 0)
				break;

			SoundIOChannelLayout layout = outstream.Layout;

			if (soundQueue.Count > 0)
			{
				int count = soundQueue.Count;
				for (int frame = 0; frame < frame_count && frame < count; frame += 1)
				{
					double sample = soundQueue.Dequeue();
						
					for (int channel = 0; channel < layout.ChannelCount; channel += 1)
					{

						var area = results.GetArea(channel);
						write_sample(area.Pointer, sample);
						area.Pointer += area.Step;
					}
				}
			}
			else
			{
				//Console.WriteLine("キューが空だよ count:" + frame_count + " min:" + frame_count_min);
				for (int frame = 0; frame < frame_count; frame += 1)
				{
						
					for (int channel = 0; channel < layout.ChannelCount; channel += 1)
					{

						var area = results.GetArea(channel);
						write_sample(area.Pointer, 0);
						area.Pointer += area.Step;
					}
				}
			}
			
			outstream.EndWrite();
			
			frames_left -= frame_count;
			if (frames_left <= 0)
				break;
		}
	}

	static unsafe void write_sample_s16ne(IntPtr ptr, double sample)
	{
		short* buf = (short*)ptr;
		double range = (double)short.MaxValue - (double)short.MinValue;
		double val = sample * range / 2.0;
		*buf = (short)val;
	}

	static unsafe void write_sample_s32ne(IntPtr ptr, double sample)
	{
		int* buf = (int*)ptr;
		double range = (double)int.MaxValue - (double)int.MinValue;
		double val = sample * range / 2.0;
		*buf = (int)val;
	}

	static unsafe void write_sample_float32ne(IntPtr ptr, double sample)
	{
		float* buf = (float*)ptr;
		*buf = (float)sample;
	}

	static unsafe void write_sample_float64ne(IntPtr ptr, double sample)
	{
		double* buf = (double*)ptr;
		*buf = sample;
	}
	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		lock (renderLock)
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
				sound = new MetasiaSound(2, 44100, 60),
				targetSize = new SKSize(3840, 2160),
				ResolutionLevel = 0.1f,
				AudioChannel = 2
			};
			renderer.Render(ref exp, VM.Frame);
			
			for(int i = 0; i < exp.sound.Pulse.Length; i++)
			{
				soundQueue.Enqueue(exp.sound.Pulse[i]);
			}
			
			canvas.DrawBitmap(exp.bitmap, 0, 0);
		}
		


	}
	
}