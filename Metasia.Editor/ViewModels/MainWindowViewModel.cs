using System;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels.Notifications;

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

        public MainWindowViewModel(
            PlayerParentViewModel playerParentVM,
            TimelineParentViewModel timelineParentViewModel,
            InspectorViewModel inspectorViewModel,
            ToolsViewModel toolsVM,
            NotificationCenterViewModel notificationCenterViewModel,
            IKeyBindingService keyBindingService)
        {
            PlayerParentVM = playerParentVM;
            TimelineParentVM = timelineParentViewModel;
            InspectorVM = inspectorViewModel;
            ToolsVM = toolsVM;
            NotificationCenterVM = notificationCenterViewModel;
            _keyBindingService = keyBindingService;
        }
    }
}
