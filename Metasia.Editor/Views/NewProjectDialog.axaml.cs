


using System;
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
using SkiaSharp;

namespace Metasia.Editor.Views
{
    public partial class NewProjectDialog : Window
    {
        public string ProjectName { get; private set; } = string.Empty;
        public string ProjectPath { get; private set; } = string.Empty;
        public ProjectInfo ProjectInfo { get; private set; }
        public MetasiaProject? SelectedTemplate { get; private set; }

        private readonly List<IProjectTemplate> _availableTemplates = new();

        public NewProjectDialog()
        {
            InitializeComponent();
        }

        public void SetTemplates(List<IProjectTemplate> templates)
        {
            _availableTemplates.Clear();
            _availableTemplates.AddRange(templates);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            // テンプレート選択コンボボックスにテンプレート名を追加
            var templateComboBox = this.FindControl<ComboBox>("TemplateComboBox");

            // 空のプロジェクトオプションはコードで処理するため、ComboBoxのItemsコレクションをクリア
            templateComboBox.Items.Clear();
            templateComboBox.Items.Add(new ComboBoxItem { Content = "空のプロジェクト" });

            foreach (var template in _availableTemplates)
            {
                templateComboBox.Items.Add(new ComboBoxItem { Content = template.Name });
            }
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


