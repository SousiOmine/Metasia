using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Services;
using ReactiveUI;
using SkiaSharp;

namespace Metasia.Editor.ViewModels
{
    public class NewProjectDialogViewModel : ViewModelBase
    {
        private string _projectName = string.Empty;
        public string ProjectName
        {
            get => _projectName;
            set => this.RaiseAndSetIfChanged(ref _projectName, value);
        }

        private string _folderPath = string.Empty;
        public string FolderPath
        {
            get => _folderPath;
            set => this.RaiseAndSetIfChanged(ref _folderPath, value);
        }

        private int _selectedFramerateIndex = 1;
        public int SelectedFramerateIndex
        {
            get => _selectedFramerateIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedFramerateIndex, value);
        }

        private int _selectedResolutionIndex = 1;
        public int SelectedResolutionIndex
        {
            get => _selectedResolutionIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedResolutionIndex, value);
        }

        private int _selectedTemplateIndex = 0;
        public int SelectedTemplateIndex
        {
            get => _selectedTemplateIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedTemplateIndex, value);
        }

        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => this.RaiseAndSetIfChanged(ref _canCreate, value);
        }

        public ICommand BrowseFolderCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CreateCommand { get; }

        private readonly IFileDialogService _fileDialogService;
        private readonly List<IProjectTemplate> _availableTemplates = new();

        public ProjectInfo ProjectInfo { get; private set; }
        public MetasiaProject? SelectedTemplate { get; private set; }
        public string ProjectPath => Path.Combine(FolderPath, ProjectName);

        public NewProjectDialogViewModel(IFileDialogService fileDialogService)
        {
            _fileDialogService = fileDialogService;
            
            BrowseFolderCommand = ReactiveCommand.CreateFromTask(BrowseFolderAsync);
            CancelCommand = ReactiveCommand.Create(() => false);
            CreateCommand = ReactiveCommand.Create(() => {
                PrepareProjectInfo();
                return true;
            });

            LoadTemplates();
            
            this.WhenAnyValue(x => x.ProjectName, x => x.FolderPath)
                .Subscribe(_ => UpdateCanCreate());
        }

        private async Task BrowseFolderAsync()
        {
            var folder = await _fileDialogService.OpenFolderDialogAsync();
            if (folder != null)
            {
                FolderPath = folder.Path.LocalPath;
            }
        }

        private void UpdateCanCreate()
        {
            CanCreate = !string.IsNullOrWhiteSpace(ProjectName) && !string.IsNullOrWhiteSpace(FolderPath);
        }

        private void LoadTemplates()
        {
            _availableTemplates.Clear();
            _availableTemplates.Add(new KariProjectTemplate());
            // 将来的に他のテンプレートを追加する場合はここに追加
        }

        private void PrepareProjectInfo()
        {
            // フレームレートを取得
            int framerate = 30;
            switch (SelectedFramerateIndex)
            {
                case 0: framerate = 24; break;
                case 1: framerate = 30; break;
                case 2: framerate = 60; break;
            }
            
            // 解像度を取得
            SKSize size = new SKSize(1920, 1080);
            switch (SelectedResolutionIndex)
            {
                case 0: size = new SKSize(1280, 720); break;
                case 1: size = new SKSize(1920, 1080); break;
                case 2: size = new SKSize(3840, 2160); break;
            }
            
            ProjectInfo = new ProjectInfo
            {
                Framerate = framerate,
                Size = size
            };

            // テンプレートを取得
            if (SelectedTemplateIndex > 0)
            {
                int templateIndex = SelectedTemplateIndex - 1; // 最初の項目は「空のプロジェクト」
                if (templateIndex >= 0 && templateIndex < _availableTemplates.Count)
                {
                    SelectedTemplate = _availableTemplates[templateIndex].Template;
                }
            }
            
            // プロジェクトフォルダを作成
            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
            }
        }
    }
}