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

        public ObservableCollection<ClipViewModel> SelectClip { get; } = new();
        
        public int Frame
        {
            get => _frame;
            set => this.RaiseAndSetIfChanged(ref _frame, value);
        }

        public double CursorLeft
        {
            get => _cursorLeft;
            set => this.RaiseAndSetIfChanged(ref _cursorLeft, value);
        }

        private TimelineObject _timeline;
        private double _frame_per_DIP;
        private int _frame;
        private double _cursorLeft;

        public TimelineViewModel(TimelineObject targetTimeline, PlayerViewModel playerViewModel)
        {
            Frame_Per_DIP = 3;
            _timeline = targetTimeline;
            
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

            SelectClip.CollectionChanged += ((sender, args) =>
            {
                foreach (var layerCanvas in LayerCanvas)
                {
                    layerCanvas.ResetSelectedClip();
                }
                foreach (var targetClip in SelectClip)
                {
                    targetClip.IsSelecting = true;
                }
            });
        }

        public void ClipSelect(ClipViewModel clip)
        {
            SelectClip.Clear();
            SelectClip.Add(clip);
        }
        
        private void ChangeFramePerDIP()
        {
            CursorLeft = Frame * Frame_Per_DIP;
        }
    }
}
