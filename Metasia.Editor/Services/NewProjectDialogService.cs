

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Views;
using SkiaSharp;

namespace Metasia.Editor.Services
{
    public class NewProjectDialogService : INewProjectDialogService
    {
        private readonly List<IProjectTemplate> _availableTemplates = new();

        public NewProjectDialogService()
        {
            LoadTemplates();
        }

        public async Task<(bool, string, ProjectInfo, MetasiaProject?)> ShowDialogAsync()
        {
            var window = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window == null)
            {
                return (false, string.Empty, null, null);
            }

            var dialog = new NewProjectDialog();
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                return (true, dialog.ProjectPath, dialog.ProjectInfo, dialog.SelectedTemplate);
            }

            return (false, string.Empty, null, null);
        }

        private void LoadTemplates()
        {
            // 利用可能なテンプレートをロード
            _availableTemplates.Clear();
            _availableTemplates.Add(new KariProjectTemplate());

            // 将来的に他のテンプレートを追加する場合はここに追加
        }
    }
}
