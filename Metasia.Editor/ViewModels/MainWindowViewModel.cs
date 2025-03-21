using System.Collections.ObjectModel;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using SkiaSharp;
using System.IO;
using System;
using System.Diagnostics;
using System.Text.Json;
using Metasia.Core.Json;
using System.Windows.Input;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Services;
using System.Threading.Tasks;
using Metasia.Editor.Models;
using Metasia.Editor.Views;
using Avalonia.Controls;

namespace Metasia.Editor.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string Greeting => "Welcome to Avalonia!";

		//public PlayerViewModel playerViewModel { get; }
		
		public PlayerParentViewModel PlayerParentVM { get; }

		public InspectorViewModel inspectorViewModel { get; }
		
		public TimelineParentViewModel TimelineParentVM { get; }
		
		public ToolsViewModel ToolsVM { get; }

		public ICommand SaveEditingProject { get; }
		public ICommand LoadEditingProject { get; }
		public ICommand CreateNewProject { get; }


		public MainWindowViewModel()
		{
			
			ToolsVM = new ToolsViewModel();

			SaveEditingProject = ReactiveCommand.Create(SaveEditingProjectExecuteAsync);
			LoadEditingProject = ReactiveCommand.Create(LoadEditingProjectExecuteAsync);
			CreateNewProject = ReactiveCommand.Create(CreateNewProjectExecuteAsync);
			ProjectInfo info = new ProjectInfo()
		    {
	    	    Framerate = 60,
	    		Size = new SKSize(3840, 2160),
	    	};
			MetasiaProject kariProject = new MetasiaProject(info);
			kariProject.LastFrame = 239;

			kariHelloObject kariHello = new kariHelloObject("karihello")
	    	{ 
	    		EndFrame = 120,
	    	};
            kariHello.Rotation.Params.Add(new CoordPoint() { Value = 90, Frame = 120 });

            kariHelloObject kariHello2 = new kariHelloObject("karihello2")
	    	{
	    		EndFrame = 10,
	    	};
            kariHello2.Y.Params[0].Value = 300;
            kariHello2.Rotation.Params[0].Value = 45;
            kariHello2.Alpha.Params[0].Value = 50;
            kariHello2.Scale.Params[0].Value = 50;
            kariHello2.X.Params.Add(new CoordPoint() { Value = 1000, Frame = 10 });

            Text text = new Text("konnichiwa")
			{
                EndFrame = 120,
				TypefaceName = "LINE Seed JP_TTF",
                Contents = "こんにちは Hello",
			};
            text.TextSize.Params[0].Value = 400;

            Text onesec = new Text("sec1")
			{
                EndFrame = 59,
                TypefaceName = "LINE Seed JP_TTF",
                Contents = "1",
            };
			onesec.TextSize.Params[0].Value = 200;
			onesec.X.Params[0].Value = -1800;
			onesec.Y.Params[0].Value = 900;

			Text twosec = new Text("sec2")
			{
				StartFrame = 60,
				EndFrame = 119,
				TypefaceName = "LINE Seed JP_TTF",
				Contents = "2",
			};
            twosec.TextSize.Params[0].Value = 200;
            twosec.X.Params[0].Value = -1800;
            twosec.Y.Params[0].Value = 900;

			Text foursec = new Text("sec4")
			{
				StartFrame = 180,
				EndFrame = 239,
				TypefaceName = "LINE Seed JP_TTF",
				Contents = "4",
			};
            foursec.TextSize.Params[0].Value = 200;
            foursec.X.Params[0].Value = -1800;
            foursec.Y.Params[0].Value = 900;

            LayerObject layer1 = new LayerObject("layer1", "Layer 1");
			LayerObject layer2 = new LayerObject("layer2", "Layer 2");
			LayerObject layer3 = new LayerObject("layer3", "Layer 3");
			LayerObject layer4 = new LayerObject("layer4", "Layer 4");
            LayerObject layer5 = new LayerObject("layer5", "Layer 5");

			TimelineObject secondTL = new TimelineObject("SecondTimeline")
			{
                StartFrame = 60,
                EndFrame = 119,
            };
			LayerObject secLayer = new LayerObject("secLayer", "Layer 1");
            secondTL.Layers.Add(secLayer);

            kariHelloObject karisec = new kariHelloObject("karihello3")
            {
                EndFrame = 1200,
            };
            karisec.Scale.Params[0].Value = 300;

            secLayer.Objects.Add(karisec);


            layer5.Objects.Add(secondTL);



            TimelineObject mainTL = new TimelineObject("RootTimeline");

			layer1.Objects.Add(kariHello);
			layer2.Objects.Add(kariHello2);
			layer3.Objects.Add(text);
			layer4.Objects.Add(onesec);
			layer4.Objects.Add(twosec);
            layer4.Objects.Add(foursec);
            mainTL.Layers.Add(layer1);
			mainTL.Layers.Add(layer2);
			mainTL.Layers.Add(layer3);
			mainTL.Layers.Add(layer4);
            mainTL.Layers.Add(layer5);

			kariProject.Timelines.Add(mainTL);
			kariProject.Timelines.Add(secondTL);

			string jsonString = ProjectSerializer.SerializeToMTPJ(kariProject);

			MetasiaProject deserializedProject = ProjectSerializer.DeserializeFromMTPJ(jsonString);

			PlayerParentVM = new PlayerParentViewModel(deserializedProject);
			PlayerParentVM.CurrentProjectStructureMethod = ProjectStructureMethod.MTPJ;

			TimelineParentVM = new TimelineParentViewModel(PlayerParentVM);

			inspectorViewModel = new InspectorViewModel(PlayerParentVM);



		}

		private async Task CreateNewProjectExecuteAsync()
		{
			try
			{
				var window = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
					? desktop.MainWindow
					: null;
				
				if (window == null) return;
				
				var dialog = new NewProjectDialog();
				var result = await dialog.ShowDialog<bool>(window);
				
				if (result)
				{
					PlayerParentVM.CreateNewProject(dialog.ProjectName, dialog.ProjectPath, dialog.ProjectInfo);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"新規プロジェクト作成エラー: {ex.Message}");
			}
		}

		private async Task SaveEditingProjectExecuteAsync()
		{
			try
			{
				var fileDialogService = App.Current?.Services?.GetService<IFileDialogService>();
				if(fileDialogService is null) throw new NullReferenceException("FileDialogService is not found");
				var file = await fileDialogService.SaveFileDialogAsync();
				if (file == null) return;
				PlayerParentVM.SaveCurrentProject(file.Path.LocalPath);
			}
			catch (Exception ex)
			{

			}
		}

		private async Task LoadEditingProjectExecuteAsync()
		{
			var fileDialogService = App.Current?.Services?.GetService<IFileDialogService>();
			if(fileDialogService is null) throw new NullReferenceException("FileDialogService is not found");
			var file = await fileDialogService.OpenFileDialogAsync();
			if(file is null) return;
			PlayerParentVM.LoadProjectFromFilePath(file.Path.LocalPath);
		}
	}
}

