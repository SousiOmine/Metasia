namespace Metasia.Core.Objects
{
    [Serializable]
    public class ClipObject : IMetasiaObject
    {
        /// <summary>
        /// オブジェクト固有のID
        /// </summary>
        public string Id { get; set; } = String.Empty;

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
        public bool IsActive { get; set; } = true;

		/// <summary>
		/// オブジェクトの初期化
		/// </summary>
		/// <param name="id">オブジェクト固有のID</param>

		public ClipObject(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));
            }
            Id = id;
        }

        public ClipObject()
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