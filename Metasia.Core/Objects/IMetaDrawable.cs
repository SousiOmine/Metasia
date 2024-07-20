using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
	/// <summary>
	/// 描画機能を持つオブジェクト用のインターフェース
	/// </summary>
	public interface IMetaDrawable
	{
		public void DrawExpresser(ref DrawExpresserArgs e, int frame);
	}
}
