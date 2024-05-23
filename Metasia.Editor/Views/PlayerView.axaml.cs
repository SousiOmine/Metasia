using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Editor.Models;
using Metasia.Editor.ViewModels;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
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
				targetSize = new SKSize(3840, 2160),
				ResolutionLevel = 0.1f
			};
			renderer.Render(ref exp, VM.Frame);
			canvas.DrawBitmap(exp.bitmap, 0, 0);
		}
		


	}
	
}