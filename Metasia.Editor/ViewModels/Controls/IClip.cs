using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Controls
{
    public interface IClip
    {
        public double Width { get; set; }

        public int FrameCount();
    }
}
