using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
	internal interface IMetaDrawable
	{
		public void DrawExpresser(ref DrawExpresserArgs e, ulong frame);
	}
}
