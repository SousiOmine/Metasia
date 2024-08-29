using System.Collections.ObjectModel;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string Greeting => "Welcome to Avalonia!";

		public PlayerViewModel playerViewModel { get; } = new();

		public InspectorViewModel inspectorViewModel { get; }

		public TimelineTabsViewModel timelineTabsViewModel { get; }



		public MainWindowViewModel()
		{
			timelineTabsViewModel = new TimelineTabsViewModel(playerViewModel);
			inspectorViewModel = new InspectorViewModel(playerViewModel);
		}
	}
}
