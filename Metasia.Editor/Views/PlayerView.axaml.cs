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


    public PlayerView()
    {
        InitializeComponent();
		
		this.DataContextChanged += (s, e) =>
		{
			VM.ViewPaintRequest = () => { skiaCanvas.InvalidateSurface(); };
		};
	}

	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
		if (renderer is null && MetasiaProvider.MetasiaProject is not null)
		{
			renderer = new ProjectRenderer(MetasiaProvider.MetasiaProject);
		}
		SKImageInfo info = e.Info;
		SKSurface surface = e.Surface;
		SKCanvas canvas = surface.Canvas;
		canvas.Clear(SKColors.Green);
		renderer.Render(ref canvas, VM.frame);
        
	}
	
}