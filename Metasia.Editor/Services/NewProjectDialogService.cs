

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

            if (window is null)
            {
                return (false, string.Empty, new ProjectInfo(60, new SKSize(1920, 1080), 44100, 2), null);
            }

            var dialog = new NewProjectDialog();
            // Pass the loaded templates to the dialog
            dialog.SetTemplates(_availableTemplates);
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                return (true, dialog.ProjectPath, dialog.ProjectInfo ?? new ProjectInfo(dialog.ProjectInfo.Framerate, dialog.ProjectInfo.Size, 44100, 2), dialog.SelectedTemplate);
            }

            return (false, string.Empty, new ProjectInfo(dialog.ProjectInfo.Framerate, dialog.ProjectInfo.Size, 44100, 2), null);
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
