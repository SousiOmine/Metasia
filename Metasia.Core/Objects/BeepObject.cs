using Metasia.Core.Render;
using Metasia.Core.Sounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
	public class BeepObject : MetasiaObject
	{
		public BeepObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			MetasiaSound sound = MetasiaSound.CreateMetasiaSound();
			for(int i = 0; i < sound.Pulse.Length; i+=2)
			{
				sound.Pulse[i] = (short)(32760 * Math.Sin((2 * Math.PI * 440) / sound.Pulse.Length * i/2));
				sound.Pulse[i + 1] = (short)(32760 * Math.Sin((2 * Math.PI * 440) / sound.Pulse.Length * i/2));
			}
			e.sound = sound;
			base.Expression(ref e, frame);
		}
	}
}
