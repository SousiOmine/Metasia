using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using Metasia.Editor.Models;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services.Notification;

namespace Metasia.Editor.Services
{
    public class AutoSaveService : IAutoSaveService, IDisposable
    {
        private readonly ISettingsService _settingsService;
        private readonly IProjectState _projectState;
        private readonly INotificationService _notificationService;
        private DispatcherTimer? _autoSaveTimer;
        private DispatcherTimer? _backupTimer;
        private bool _disposed;

        public AutoSaveService(ISettingsService settingsService, IProjectState projectState, INotificationService notificationService)
        {
            _settingsService = settingsService;
            _projectState = projectState;
            _notificationService = notificationService;

            _settingsService.SettingsChanged += OnSettingsChanged;
        }

        public void Start()
        {
            Stop();
            UpdateAutoSaveTimer();
            UpdateBackupTimer();
        }

        public void Stop()
        {
            StopAutoSaveTimer();
            StopBackupTimer();
        }

        public void StartBackup()
        {
            UpdateBackupTimer();
        }

        public void StopBackup()
        {
            StopBackupTimer();
        }

        private void StopBackupTimer()
        {
            if (_backupTimer != null)
            {
                _backupTimer.Tick -= OnBackupTimerTick;
                _backupTimer.Stop();
                _backupTimer = null;
            }
        }

        private void StopAutoSaveTimer()
        {
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Tick -= OnAutoSaveTimerTick;
                _autoSaveTimer.Stop();
                _autoSaveTimer = null;
            }
        }

        private void UpdateAutoSaveTimer()
        {
            StopAutoSaveTimer();

            var autoSaveEnabled = _settingsService.CurrentSettings.General.AutoSave;
            if (!autoSaveEnabled)
            {
                return;
            }

            var intervalMinutes = _settingsService.CurrentSettings.General.AutoSaveInterval;
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            _autoSaveTimer = new DispatcherTimer
            {
                Interval = interval
            };
            _autoSaveTimer.Tick += OnAutoSaveTimerTick;
            _autoSaveTimer.Start();
        }

        private void UpdateBackupTimer()
        {
            StopBackup();

            var autoBackupEnabled = _settingsService.CurrentSettings.General.AutoBackup;
            if (!autoBackupEnabled)
            {
                return;
            }

            var intervalMinutes = _settingsService.CurrentSettings.General.AutoBackupInterval;
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            _backupTimer = new DispatcherTimer
            {
                Interval = interval
            };
            _backupTimer.Tick += OnBackupTimerTick;
            _backupTimer.Start();
        }

        private void OnSettingsChanged()
        {
            UpdateAutoSaveTimer();
            UpdateBackupTimer();
        }

        private void OnAutoSaveTimerTick(object? sender, EventArgs e)
        {
            var currentProject = _projectState.CurrentProject;
            if (currentProject == null)
            {
                return;
            }

            var projectFilePath = currentProject.ProjectFilePath;
            if (string.IsNullOrEmpty(projectFilePath))
            {
                return;
            }

            try
            {
                ProjectSaveLoadManager.Save(currentProject, projectFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoSaveService] Auto save failed for project: {projectFilePath}\n{ex}");
                _notificationService.ShowError(
                    "自動保存失敗",
                    $"プロジェクトの自動保存に失敗しました。\n{ex.Message}"
                );
            }
        }

        private void OnBackupTimerTick(object? sender, EventArgs e)
        {
            var currentProject = _projectState.CurrentProject;
            if (currentProject == null)
            {
                return;
            }

            var projectFilePath = currentProject.ProjectFilePath;
            if (string.IsNullOrEmpty(projectFilePath))
            {
                return;
            }

            try
            {
                CreateBackup(currentProject, projectFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoSaveService] Auto backup failed for project: {projectFilePath}\n{ex}");
                _notificationService.ShowError(
                    "自動バックアップ失敗",
                    $"プロジェクトの自動バックアップに失敗しました。\n{ex.Message}"
                );
            }
        }

        private void CreateBackup(MetasiaEditorProject project, string projectFilePath)
        {
            var settings = _settingsService.CurrentSettings.General;
            var backupPath = settings.AutoBackupPath;

            if (string.IsNullOrEmpty(backupPath))
            {
                var projectDir = Path.GetDirectoryName(projectFilePath);
                if (string.IsNullOrEmpty(projectDir))
                {
                    return;
                }
                backupPath = Path.Combine(projectDir, "backup");
            }

            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            var projectFileName = Path.GetFileNameWithoutExtension(projectFilePath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            var backupFileName = $"{projectFileName}_{timestamp}.mtpj";
            var backupFilePath = Path.Combine(backupPath, backupFileName);

            ProjectSaveLoadManager.Save(project, backupFilePath);

            CleanupOldBackups(backupPath, projectFileName, settings.AutoBackupMaxCount);
        }

        private void CleanupOldBackups(string backupPath, string projectFileName, int maxCount)
        {
            if (maxCount <= 0)
            {
                return;
            }

            var pattern = $"{projectFileName}_*.mtpj";
            var backupFiles = Directory.GetFiles(backupPath, pattern)
                .OrderBy(f => f)
                .ToList();

            while (backupFiles.Count > maxCount)
            {
                var fileToDelete = backupFiles.First();
                File.Delete(fileToDelete);
                backupFiles.RemoveAt(0);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _settingsService.SettingsChanged -= OnSettingsChanged;
                    StopAutoSaveTimer();
                    StopBackupTimer();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}