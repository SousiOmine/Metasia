using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
	/// <summary>
	/// 音声を持つオブジェクト用のインターフェース
	/// </summary>
	public interface IMetaAudiable
	{
		public double Volume { get; set; }
		public void AudioExpresser(ref AudioExpresserArgs e,  int frame);
	}
}
