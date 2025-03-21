using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Metasia.Core.Project;
using SkiaSharp;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        public string ProjectName { get; private set; } = string.Empty;
        public string ProjectPath { get; private set; } = string.Empty;
        public ProjectInfo ProjectInfo { get; private set; }
        
        public NewProjectDialog()
        {
            InitializeComponent();

            var browseButton = this.FindControl<Button>("BrowseButton");
            var folderPathTextBox = this.FindControl<TextBox>("FolderPathTextBox");
            var projectNameTextBox = this.FindControl<TextBox>("ProjectNameTextBox");
            var cancelButton = this.FindControl<Button>("CancelButton");
            var createButton = this.FindControl<Button>("CreateButton");
            var framerateComboBox = this.FindControl<ComboBox>("FramerateComboBox");
            var resolutionComboBox = this.FindControl<ComboBox>("ResolutionComboBox");

            browseButton.Click += async (sender, e) =>
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "保存先フォルダを選択",
                    AllowMultiple = false
                });

                if (folders.Count > 0)
                {
                    folderPathTextBox.Text = folders[0].Path.LocalPath;
                    UpdateCreateButtonState();
                }
            };

            projectNameTextBox.TextChanged += (sender, e) =>
            {
                UpdateCreateButtonState();
            };

            cancelButton.Click += (sender, e) =>
            {
                Close(false);
            };

            createButton.Click += (sender, e) =>
            {
                ProjectName = projectNameTextBox.Text;
                ProjectPath = Path.Combine(folderPathTextBox.Text, ProjectName);
                
                // フレームレートを取得
                int framerate = 30;
                switch (framerateComboBox.SelectedIndex)
                {
                    case 0: framerate = 24; break;
                    case 1: framerate = 30; break;
                    case 2: framerate = 60; break;
                }
                
                // 解像度を取得
                SKSize size = new SKSize(1920, 1080);
                switch (resolutionComboBox.SelectedIndex)
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
                
                // プロジェクトフォルダを作成
                if (!Directory.Exists(ProjectPath))
                {
                    Directory.CreateDirectory(ProjectPath);
                }
                
                Close(true);
            };
        }

        private void UpdateCreateButtonState()
        {
            var projectNameTextBox = this.FindControl<TextBox>("ProjectNameTextBox");
            var folderPathTextBox = this.FindControl<TextBox>("FolderPathTextBox");
            var createButton = this.FindControl<Button>("CreateButton");
            
            bool hasProjectName = !string.IsNullOrWhiteSpace(projectNameTextBox.Text);
            bool hasFolderPath = !string.IsNullOrWhiteSpace(folderPathTextBox.Text);
            
            createButton.IsEnabled = hasProjectName && hasFolderPath;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 