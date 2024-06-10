using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
	internal interface IMetaAudiable
	{
		public void AudioExpresser(ref AudioExpresserArgs e, ulong frame);
	}
}
