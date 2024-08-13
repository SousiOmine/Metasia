using Avalonia.Controls;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels
{
    public class TimelineTabsViewModel : ViewModelBase
    {
        public ObservableCollection<TabItem> Tabs { get; } = new();

        public TimelineViewModel FocusTimelineViewModel;

        private ObservableCollection<TimelineObject> _projectTimelines = new();

        public TimelineTabsViewModel()
        {
            foreach (var timeline in MetasiaProvider.MetasiaProject.Timelines)
            {
                //Tabs.Add(new TabItem { Header = timeline.Id, Content = new TimelineViewModel()});
                //Tabs.Add(new TabItem { Header = "タブその2" } );
                //Tabs.Add(new TabItem { Header = "タブその3" } );
                _projectTimelines.Add(timeline);
            }

            foreach(var timeline in _projectTimelines)
            {
                TimelineViewModel timelineViewModel = new TimelineViewModel();
                timelineViewModel.Timeline = timeline;
                Tabs.Add(new TabItem { Header = timeline.Id, Content = timelineViewModel});
            }
        }
    }
}
