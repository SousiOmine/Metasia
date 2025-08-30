using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using ReactiveUI;
using SkiaSharp;

namespace Metasia.Editor.ViewModels
{
	public class NewProjectDialogViewModel : ViewModelBase
	{
		private string _projectName = string.Empty;
		private string _folderPath = string.Empty;
		private int _selectedFramerateIndex = 1;
		private int _selectedResolutionIndex = 1;
		private int _selectedTemplateIndex = 0;
		private readonly List<IProjectTemplate> _availableTemplates = new();

		public string ProjectName
		{
			get => _projectName;
			set => this.RaiseAndSetIfChanged(ref _projectName, value);
		}

		public string FolderPath
		{
			get => _folderPath;
			set => this.RaiseAndSetIfChanged(ref _folderPath, value);
		}

		public int SelectedFramerateIndex
		{
			get => _selectedFramerateIndex;
			set => this.RaiseAndSetIfChanged(ref _selectedFramerateIndex, value);
		}

		public int SelectedResolutionIndex
		{
			get => _selectedResolutionIndex;
			set => this.RaiseAndSetIfChanged(ref _selectedResolutionIndex, value);
		}

		public int SelectedTemplateIndex
		{
			get => _selectedTemplateIndex;
			set => this.RaiseAndSetIfChanged(ref _selectedTemplateIndex, value);
		}

		public List<IProjectTemplate> AvailableTemplates => _availableTemplates;

		public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }
		public ReactiveCommand<Unit, bool> CreateCommand { get; }
		public ReactiveCommand<Unit, bool> CancelCommand { get; }

		public string ProjectPath => Path.Combine(FolderPath, ProjectName);

		public ProjectInfo ProjectInfo
		{
			get
			{
				// フレームレートを取得
				int framerate = SelectedFramerateIndex switch
				{
					0 => 24,
					1 => 30,
					2 => 60,
					_ => 30
				};

				// 解像度を取得
				SKSize size = SelectedResolutionIndex switch
				{
					0 => new SKSize(1280, 720),
					1 => new SKSize(1920, 1080),
					2 => new SKSize(3840, 2160),
					_ => new SKSize(1920, 1080)
				};

				return new ProjectInfo
				{
					Framerate = framerate,
					Size = size
				};
			}
		}

		public MetasiaProject? SelectedTemplate
		{
			get
			{
				if (SelectedTemplateIndex > 0)
				{
					int templateIndex = SelectedTemplateIndex - 1; // 最初の項目は「空のプロジェクト」
					if (templateIndex >= 0 && templateIndex < _availableTemplates.Count)
					{
						return _availableTemplates[templateIndex].Template;
					}
				}
				return null;
			}
		}

		public NewProjectDialogViewModel()
		{
			BrowseFolderCommand = ReactiveCommand.CreateFromTask(BrowseFolderAsync);
			var canCreate = this.WhenAnyValue(x => x.ProjectName, x => x.FolderPath)
				.Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2));
			CreateCommand = ReactiveCommand.Create(() => true, canCreate);
			CancelCommand = ReactiveCommand.Create(() => false);
		}

		public void SetTemplates(List<IProjectTemplate> templates)
		{
			_availableTemplates.Clear();
			_availableTemplates.AddRange(templates);
			this.RaisePropertyChanged(nameof(AvailableTemplates));
		}

		private async Task BrowseFolderAsync()
		{
			// Note: This is a placeholder. In a real implementation, 
			// the actual folder picker logic would be handled by the View or a service.
			// For now, we'll just raise a property changed event to indicate the folder path might have changed.
			this.RaisePropertyChanged(nameof(FolderPath));
		}
	}
}