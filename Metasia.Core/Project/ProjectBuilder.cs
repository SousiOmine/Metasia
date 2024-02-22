using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Metasia.Core.Project
{
	public class ProjectBuilder
	{
		/// <summary>
		/// 空のフォルダに空のMetasiaProjectを作成する
		/// </summary>
		/// <param name="folder_path">プロジェクトフォルダとするパス</param>
		/// <returns></returns>
		public static bool CreateFromTemplate(string folder_path)
		{
			if (!Directory.Exists(folder_path))
			{
				return false;
			};

			ProjectInfo info = new ProjectInfo()
			{
				Framerate = 60,
				Size = new SKSize(1920, 1080),
			};

			var jsonoptions = new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),

				//単語間のスペースを_に置き換える
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,

				WriteIndented = true
			};

			var jsonString = JsonSerializer.Serialize(info, jsonoptions);

			using (StreamWriter sw = new StreamWriter(Path.Combine(folder_path, "project.metasia")))
			{
				sw.Write(jsonString);
			}


			return true;
		}
	}
}
