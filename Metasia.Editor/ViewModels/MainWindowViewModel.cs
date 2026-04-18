using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels.Notifications;
using System.IO;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public PlayerParentViewModel PlayerParentVM { get; }
        public InspectorViewModel InspectorVM { get; }
        public TimelineParentViewModel TimelineParentVM { get; }
        public ToolsViewModel ToolsVM { get; }
        public NotificationCenterViewModel NotificationCenterVM { get; }

        private readonly IKeyBindingService _keyBindingService;
        private readonly IProjectState _projectState;

        private string _title = "Metasia";
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public MainWindowViewModel(
            PlayerParentViewModel playerParentVM,
            TimelineParentViewModel timelineParentViewModel,
            InspectorViewModel inspectorViewModel,
            ToolsViewModel toolsVM,
            NotificationCenterViewModel notificationCenterViewModel,
            IKeyBindingService keyBindingService,
            IProjectState projectState)
        {
            PlayerParentVM = playerParentVM;
            TimelineParentVM = timelineParentViewModel;
            InspectorVM = inspectorViewModel;
            ToolsVM = toolsVM;
            NotificationCenterVM = notificationCenterViewModel;
            _keyBindingService = keyBindingService;
            _projectState = projectState;

            _projectState.ProjectLoaded += UpdateTitle;
            _projectState.ProjectClosed += UpdateTitle;
            _projectState.IsDirtyChanged += UpdateTitle;
        }

        private void UpdateTitle()
        {
            var projectName = _projectState.CurrentProject?.ProjectFilePath is string path
                ? Path.GetFileNameWithoutExtension(path)
                : "新規プロジェクト";

            Title = _projectState.IsDirty
                ? $"*{projectName} - Metasia"
                : $"{projectName} - Metasia";
        }
    }
}
