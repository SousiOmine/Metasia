using Metasia.Core.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Controls
{
    public class ClipViewModel : ViewModelBase, IClip
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

        private double width;

        public ClipViewModel(MetasiaObject targetObject)
        {
            TargetObject = targetObject;
            Width = 200;
        }

        public int FrameCount()
        {
            return TargetObject.EndFrame - TargetObject.StartFrame;
        }
    }
}
