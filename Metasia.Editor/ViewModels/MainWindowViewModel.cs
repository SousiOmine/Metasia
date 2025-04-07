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
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Models.EditCommands;
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



            SaveEditingProject = ReactiveCommand.Create(SaveEditingProjectExecuteAsync);
			LoadEditingProject = ReactiveCommand.Create(LoadEditingProjectExecuteAsync);
			CreateNewProject = ReactiveCommand.Create(CreateNewProjectExecuteAsync);
			
			PlayerParentVM = new PlayerParentViewModel();

			TimelineParentVM = new TimelineParentViewModel(PlayerParentVM);

			inspectorViewModel = new InspectorViewModel(PlayerParentVM);

			ToolsVM = new ToolsViewModel(PlayerParentVM);



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
					MetasiaEditorProject editorProject = ProjectGenerator.CreateProject(dialog.ProjectPath, dialog.ProjectInfo, dialog.SelectedTemplate);
					PlayerParentVM.LoadProject(editorProject);
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
				//別の場所に保存するやつを書く
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"プロジェクト保存エラー: {ex.Message}");
			}
		}

		private async Task LoadEditingProjectExecuteAsync()
		{
			var folderDialogService = App.Current?.Services?.GetService<IFileDialogService>();
			if (folderDialogService is null) throw new NullReferenceException("FileDialogService is not found");
			var folder = await folderDialogService.OpenFolderDialogAsync();
			if (folder is null) return;

			MetasiaEditorProject editorProject = ProjectSaveLoadManager.Load(new DirectoryEntity(folder.Path.LocalPath));
			PlayerParentVM.LoadProject(editorProject);
		}
	}
}

