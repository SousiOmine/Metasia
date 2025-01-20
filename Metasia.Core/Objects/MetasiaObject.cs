using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    /// <summary>
    ///	Metasiaのタイムラインや動画、音声といったオブジェクトの基底クラス
    /// </summary>
    [JsonConverter(typeof(MetasiaObjectJsonConverter))]
    // [JsonDerivedType(typeof(kariHelloObject), typeDiscriminator: "kariHelloObject")]
    // [JsonDerivedType(typeof(Text), typeDiscriminator: "Text")]
    // [JsonDerivedType(typeof(LayerObject), typeDiscriminator: "LayerObject")]
    // [JsonDerivedType(typeof(TimelineObject), typeDiscriminator: "TimelineObject")]
    public class MetasiaObject
    {
        /// <summary>
        /// オブジェクト固有のID
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// 子オブジェクト１つ
        /// </summary>
        public MetasiaObject? Child;

        /// <summary>
        /// オブジェクトの先頭フレーム
        /// </summary>
        public int StartFrame = 0;

        /// <summary>
        /// オブジェクトの終端フレーム
        /// </summary>
        public int EndFrame = 100;

        /// <summary>
        /// オブジェクトを有効にするか
        /// </summary>
        public bool IsActive = true;

		/// <summary>
		/// オブジェクトの初期化
		/// </summary>
		/// <param name="id">オブジェクト固有のID</param>

		public MetasiaObject(string id)
        {
            Id = id;
        }

        public MetasiaObject()
        {
        }

        /// <summary>
        /// 指定したフレームにオブジェクトが存在するか否か
        /// </summary>
        /// <param name="frame">気になるフレーム</param>
        /// <returns>存在すればtrue</returns>
        public bool IsExistFromFrame(int frame)
        {
            if(frame >= StartFrame && frame <= EndFrame) return true;
            else return false;
        }
    }
}
