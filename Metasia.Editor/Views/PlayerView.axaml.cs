using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;

namespace Metasia.Editor.Views;

public partial class PlayerView : UserControl
{
	MetasiaProject proj;
	ProjectRenderer renderer;

	private int frame = 0;

    public PlayerView()
    {
        InitializeComponent();
		ProjectInfo info = new ProjectInfo()
		{
			Framerate = 60,
			Size = new SKSize(500, 300),
		};
		proj = new MetasiaProject(info);

		kariHelloObject kariHello = new kariHelloObject("karihello")
		{ 
			EndFrame = 10,
			Layer = 1
		};
		kariHelloObject kariHello2 = new kariHelloObject("karihello2")
		{
			EndFrame = 10,
			Layer = 2
		};
		kariHello2.SetY(300);
		kariHello2.SetRotation(45);
		TimelineObject mainTL = new TimelineObject("RootTimeline");
		mainTL.Objects.Add(kariHello);
		mainTL.Objects.Add(kariHello2);
		proj.Timelines.Add(mainTL);


		renderer = new ProjectRenderer(proj);
	}

	private void SKCanvasView_PaintSurface(object? sender, Avalonia.Labs.Controls.SKPaintSurfaceEventArgs e)
	{
        SKImageInfo info = e.Info;
		SKSurface surface = e.Surface;
		SKCanvas canvas = surface.Canvas;

		canvas.Clear(SKColors.Green);

		renderer.Render(ref canvas, frame);
	}

	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		frame++;
		skiaCanvas.InvalidateSurface();
	}

	
}