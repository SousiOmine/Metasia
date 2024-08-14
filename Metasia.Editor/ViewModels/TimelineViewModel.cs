using Avalonia;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<LayerButtonViewModel> LayerButtons { get; } = new();

        public int Target_Frame;

        public MetasiaObject Target_Object;

        private TimelineObject _timeline;

        public TimelineViewModel()
        {
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());
            LayerButtons.Add(new LayerButtonViewModel());

        }

        private void ScrollSync()
        {

        }
    }
}
