using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineObject Timeline
        {
            get => _timeline;
            set => this.RaiseAndSetIfChanged(ref _timeline, value);
        }

        public int Target_Frame;

        public MetasiaObject Target_Object;

        private TimelineObject _timeline;

        public TimelineViewModel()
        {

        }
    }
}
