using System.IO;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.ViewModels;
using SkiaSharp;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        private NewProjectDialogViewModel? _viewModel;

        public string ProjectName => _viewModel?.ProjectName ?? string.Empty;
        public string ProjectPath => _viewModel?.ProjectPath ?? string.Empty;
        public ProjectInfo ProjectInfo => _viewModel?.ProjectInfo ?? new ProjectInfo();
        public MetasiaProject? SelectedTemplate => _viewModel?.SelectedTemplate;

        public NewProjectDialog()
        {
            InitializeComponent();
        }

        public void SetTemplates(List<IProjectTemplate> templates)
        {
            if (_viewModel == null)
            {
                _viewModel = new NewProjectDialogViewModel();
                DataContext = _viewModel;
            }
            _viewModel.SetTemplates(templates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            // テンプレート選択コンボックスにテンプレート名を追加
            var templateComboBox = this.FindControl<ComboBox>("TemplateComboBox");

            // 空のプロジェクトオプションはコードで処理するため、ComboBoxのItemsコレクションをクリア
            templateComboBox.Items.Clear();
            templateComboBox.Items.Add(new ComboBoxItem { Content = "空のプロジェクト" });

            if (_viewModel != null)
            {
                foreach (var template in _viewModel.AvailableTemplates)
                {
                    templateComboBox.Items.Add(new ComboBoxItem { Content = template.Name });
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            // コントロールの参照を取得
            var browseButton = this.FindControl<Button>("BrowseButton");
            var folderPathTextBox = this.FindControl<TextBox>("FolderPathTextBox");
            var projectNameTextBox = this.FindControl<TextBox>("ProjectNameTextBox");
            var cancelButton = this.FindControl<Button>("CancelButton");
            var createButton = this.FindControl<Button>("CreateButton");
            var framerateComboBox = this.FindControl<ComboBox>("FramerateComboBox");
            var resolutionComboBox = this.FindControl<ComboBox>("ResolutionComboBox");
            var templateComboBox = this.FindControl<ComboBox>("TemplateComboBox");

            // ボタンクリックイベントの設定
            browseButton.Click += async (sender, e) =>
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "保存先フォルダを選択",
                    AllowMultiple = false
                });

                if (folders.Count > 0 && _viewModel != null)
                {
                    folderPathTextBox.Text = folders[0].Path.LocalPath;
                    _viewModel.FolderPath = folderPathTextBox.Text;
                }
            };

            projectNameTextBox.TextChanged += (sender, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.ProjectName = projectNameTextBox.Text;
                }
            };

            framerateComboBox.SelectionChanged += (sender, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedFramerateIndex = framerateComboBox.SelectedIndex;
                }
            };

            resolutionComboBox.SelectionChanged += (sender, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedResolutionIndex = resolutionComboBox.SelectedIndex;
                }
            };

            templateComboBox.SelectionChanged += (sender, e) =>
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedTemplateIndex = templateComboBox.SelectedIndex;
                }
            };

            cancelButton.Click += (sender, e) =>
            {
                Close(false);
            };

            createButton.Click += (sender, e) =>
            {
                if (_viewModel != null)
                {
                    // プロジェクトフォルダを作成
                    if (!Directory.Exists(ProjectPath))
                    {
                        Directory.CreateDirectory(ProjectPath);
                    }
                    Close(true);
                }
            };
        }
    }
}


