using Avalonia.Controls;
using Metasia.Core.Objects;
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
        /// <summary>
        /// タイムラインのタブに入れるアイテム
        /// </summary>
        public ObservableCollection<TabItem> Tabs { get; } = new();

        /// <summary>
        /// 現在読み込んでいるプロジェクトのタイムライン
        /// </summary>
        private ObservableCollection<TimelineObject> _projectTimelines = new();

        public TimelineTabsViewModel(PlayerViewModel playerViewModel)
        {
            /*foreach (var timeline in MetasiaProvider.MetasiaProject.Timelines)
            {
                _projectTimelines.Add(timeline);
            }

            foreach(var timeline in _projectTimelines)
            {
                TimelineViewModel timelineViewModel = new TimelineViewModel(timeline, playerViewModel);
                Tabs.Add(new TabItem { Header = timeline.Id, Content = timelineViewModel});
            }*/
        }
    }
}
