using System;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;

namespace Metasia.Editor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public PlayerParentViewModel PlayerParentVM { get; }
        public InspectorViewModel InspectorVM { get; }
        public TimelineParentViewModel TimelineParentVM { get; }
        public ToolsViewModel ToolsVM { get; }

        private readonly IKeyBindingService _keyBindingService;

        public MainWindowViewModel(
            PlayerParentViewModel playerParentVM,
            TimelineParentViewModel timelineParentViewModel,
            InspectorViewModel inspectorViewModel,
            ToolsViewModel toolsVM,
            IKeyBindingService keyBindingService)
        {
            PlayerParentVM = playerParentVM;
            TimelineParentVM = timelineParentViewModel;
            InspectorVM = inspectorViewModel;
            ToolsVM = toolsVM;
            _keyBindingService = keyBindingService;
        }
    }
}