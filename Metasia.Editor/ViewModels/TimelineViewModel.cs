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

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set => this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
        }

        public ObservableCollection<LayerButtonViewModel> LayerButtons { get; } = new();

        public ObservableCollection<LayerCanvasViewModel> LayerCanvas { get; } = new();

        public ObservableCollection<MetasiaObject> SelectClip { get; } = new();

        public int Target_Frame;

        private TimelineObject _timeline;
        private double _frame_per_DIP;

        public TimelineViewModel(TimelineObject targetTimeline)
        {
            Frame_Per_DIP = 30;
            _timeline = targetTimeline;

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, layer));
            }
        }
    }
}
