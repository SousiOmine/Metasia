using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Core.Project;
using Metasia.Editor.Services;
using Metasia.Editor.Models;
using Metasia.Editor.Models.ProjectGenerate;
using SkiaSharp;
using System.Collections.Generic;

namespace Metasia.Editor.ViewModels
{
    public class NewProjectDialogViewModel : ViewModelBase
    {
        private string _projectName = string.Empty;
        private string _folderPath = string.Empty;
        private int _selectedFramerateIndex = 1; // 30fps
        private int _selectedResolutionIndex = 1; // 1920x1080
        private int _selectedTemplateIndex = 0;

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

        public ObservableCollection<string> FramerateOptions { get; } = new()
        {
            "24 FPS", "30 FPS", "60 FPS"
        };

        public ObservableCollection<string> ResolutionOptions { get; } = new()
        {
            "1280x720", "1920x1080", "3840x2160"
        };

        public ObservableCollection<string> TemplateOptions { get; } = new()
        {
            "空のプロジェクト", "基本テンプレート"
        };

        public ICommand BrowseFolderCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public bool CanCreate => !string.IsNullOrWhiteSpace(ProjectName) && 
                                !string.IsNullOrWhiteSpace(FolderPath);

        public Action<NewProjectDialogResult>? OnDialogResult { get; set; }

        public NewProjectDialogViewModel()
        {
            BrowseFolderCommand = ReactiveCommand.CreateFromTask(BrowseFolderAsync);
            CreateCommand = ReactiveCommand.Create(Create, this.WhenAnyValue(x => x.CanCreate));
            CancelCommand = ReactiveCommand.Create(Cancel);

            // プロパティ変更時にCanCreateを更新
            this.WhenAnyValue(x => x.ProjectName, x => x.FolderPath)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanCreate)));
        }

        private async Task BrowseFolderAsync()
        {
            var fileDialogService = App.Current?.Services?.GetService<IFileDialogService>();
            if (fileDialogService != null)
            {
                var folder = await fileDialogService.OpenFolderDialogAsync();
                if (folder != null)
                {
                    FolderPath = folder.Path.LocalPath;
                }
            }
        }

        private void Create()
        {
            var projectPath = Path.Combine(FolderPath, ProjectName);
            
            var result = new NewProjectDialogResult
            {
                ProjectName = ProjectName,
                ProjectPath = projectPath,
                ProjectInfo = CreateProjectInfo(),
                SelectedTemplate = GetSelectedTemplate(),
                Success = true
            };

            OnDialogResult?.Invoke(result);
        }

        private void Cancel()
        {
            OnDialogResult?.Invoke(new NewProjectDialogResult { Success = false });
        }

        private ProjectInfo CreateProjectInfo()
        {
            int framerate = SelectedFramerateIndex switch
            {
                0 => 24,
                1 => 30,
                2 => 60,
                _ => 30
            };

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

        private MetasiaProject? GetSelectedTemplate()
        {
            if (SelectedTemplateIndex > 0)
            {
                var templates = new List<IProjectTemplate> { new KariProjectTemplate() };
                int templateIndex = SelectedTemplateIndex - 1;
                if (templateIndex >= 0 && templateIndex < templates.Count)
                {
                    return templates[templateIndex].Template;
                }
            }
            return null;
        }
    }
}