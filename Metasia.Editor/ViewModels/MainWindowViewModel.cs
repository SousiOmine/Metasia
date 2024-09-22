using System.Collections.ObjectModel;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string Greeting => "Welcome to Avalonia!";

		public PlayerViewModel playerViewModel { get; }

		public InspectorViewModel inspectorViewModel { get; }

		public TimelineTabsViewModel timelineTabsViewModel { get; }



		public MainWindowViewModel()
		{
            playerViewModel = new PlayerViewModel();
            timelineTabsViewModel = new TimelineTabsViewModel(playerViewModel);
			inspectorViewModel = new InspectorViewModel(playerViewModel);
		}
	}
}
