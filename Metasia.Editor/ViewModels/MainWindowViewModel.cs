namespace Metasia.Editor.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string Greeting => "Welcome to Avalonia!";

		public PlayerViewModel playerViewModel { get; } = new();

		public TimelineTabsViewModel timelineTabsViewModel { get; }

		public MainWindowViewModel()
		{
			timelineTabsViewModel = new TimelineTabsViewModel(playerViewModel);
		}
	}
}
