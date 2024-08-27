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
            set
            {
                this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
                ChangeFramePerDIP();
            } 
        }

        public ObservableCollection<LayerButtonViewModel> LayerButtons { get; } = new();

        public ObservableCollection<LayerCanvasViewModel> LayerCanvas { get; } = new();

        public ObservableCollection<MetasiaObject> SelectClip { get; } = new();

        public PlayerViewModel TargetPlayer;

        public int Frame
        {
            get => frame;
            set => this.RaiseAndSetIfChanged(ref frame, value);
        }

        public double CursorLeft
        {
            get => cursorLeft;
            set => this.RaiseAndSetIfChanged(ref cursorLeft, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;
        private int frame;
        private double cursorLeft;

        public TimelineViewModel(TimelineObject targetTimeline, PlayerViewModel playerViewModel)
        {
            Frame_Per_DIP = 3;
            _timeline = targetTimeline;
            
            TargetPlayer = playerViewModel;

            playerViewModel.WhenAnyValue(x => x.Frame).Subscribe
                (Frame =>
                {
                    this.Frame = Frame;
                    CursorLeft = Frame * Frame_Per_DIP;
                });

            foreach (var layer in Timeline.Layers)
            {
                LayerButtons.Add(new LayerButtonViewModel(layer));
                LayerCanvas.Add(new LayerCanvasViewModel(this, layer));
            }
        }
        
        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * Frame_Per_DIP;
        }
    }
}
