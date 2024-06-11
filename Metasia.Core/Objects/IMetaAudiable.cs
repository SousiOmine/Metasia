using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
	public interface IMetaAudiable
	{
		public float Volume { get; set; }
		public void AudioExpresser(ref AudioExpresserArgs e,  int frame);
	}
}
