using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Controls
{
    public class ClipViewModel : ViewModelBase
    {
        public MetasiaObject TargetObject
        {
            get;
            set;
        }
        public double Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
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

        public double StartFrame
        {
            get => startFrame;
            set => this.RaiseAndSetIfChanged(ref startFrame, value);
        }

        private double width;
        private double _frame_per_DIP;
        private double startFrame;

        public ClipViewModel(MetasiaObject targetObject)
        {
            TargetObject = targetObject;
        }

        private void ChangeFramePerDIP()
        {
            Width = (TargetObject.EndFrame - TargetObject.StartFrame + 1) * Frame_Per_DIP;
            StartFrame = TargetObject.StartFrame * Frame_Per_DIP;
        }
    }
}
