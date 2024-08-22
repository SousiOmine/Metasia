using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Controls
{
    public class LayerCanvasViewModel : ViewModelBase
    {
        public ObservableCollection<IClip> ClipsAndBlanks { get; set; } = new();

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            set => this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
        }

        private TimelineViewModel parentTimeline;

        private LayerObject targetLayer;

        private double _frame_per_DIP;

        public LayerCanvasViewModel(TimelineViewModel parentTimeline, LayerObject targetLayer) 
        {
            this.parentTimeline = parentTimeline;
            this.targetLayer = targetLayer;

            parentTimeline.WhenAnyValue(x => x.Frame_Per_DIP).Subscribe
                (Frame_Per_DIP =>
                {
                    this.Frame_Per_DIP = Frame_Per_DIP;
                    ChangeFramePerDIP();
                });

            foreach(var obj in targetLayer.Objects)
            {
                ClipsAndBlanks.Add(new ClipViewModel(obj));
            }

            ChangeFramePerDIP();
        }

        private void ChangeFramePerDIP()
        {
            foreach (var clip in ClipsAndBlanks)
            {
                clip.Width = clip.FrameCount() * Frame_Per_DIP;
            }
        }
    }
}
