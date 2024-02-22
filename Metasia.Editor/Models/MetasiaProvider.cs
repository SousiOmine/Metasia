using Metasia.Core.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.Models
{
	/// <summary>
	/// Metasiaプロジェクトのインスタンスを保持する
	/// </summary>
	internal class MetasiaProvider
	{
		public static MetasiaProject? MetasiaProject { get; set; }
	}
}
