using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using System.IO;
using System.Threading.Tasks;

namespace Metasia.Editor.Models;

public static class ProjectSaveHelper
{
    public static bool NeedsSaveForRelativeMedia(MetasiaEditorProject? project, ISettingsService settingsService)
    {
        return settingsService.CurrentSettings.General.MediaPathStyle == MediaPathStyle.Relative
            && project?.ProjectFilePath == null;
    }

    public static async Task<bool> EnsureProjectSavedAsync(
        IProjectState projectState,
        IFileDialogService fileDialogService)
    {
        var project = projectState.CurrentProject;
        if (project?.ProjectFilePath is not null)
            return true;

        var file = await fileDialogService.SaveFileDialogAsync(
            "プロジェクトを保存",
            ["*.mtpj"],
            "mtpj");
        if (file is null) return false;

        ProjectSaveLoadManager.Save(project, file.Path.LocalPath);
        project.ProjectFilePath = file.Path.LocalPath;
        project.ProjectPath = new DirectoryEntity(Path.GetDirectoryName(file.Path.LocalPath)!);
        projectState.IsDirty = false;
        return true;
    }
}
