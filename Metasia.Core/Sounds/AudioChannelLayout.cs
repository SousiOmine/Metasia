namespace Metasia.Core.Sounds
{
    /// <summary>
    /// 音声のチャンネルレイアウト。値はチャンネル数を表す。
    /// サンプルの並び順はWAVEFORMATEXTENSIBLEに合わせて固定する。
    /// 1chはFC、2chはFL,FR、6chはFL,FR,FC,LFE,BL,BRの順に並ぶ。
    /// </summary>
    public enum AudioChannelLayout
    {
        Mono = 1,
        Stereo = 2,
        Surround5_1 = 6,
    }

    /// <summary>
    /// <see cref="AudioChannelLayout"/>の検証と文字列表現の変換を行う
    /// </summary>
    public static class AudioChannelLayouts
    {
        /// <summary>
        /// 対応している最大チャンネル数
        /// </summary>
        public const int MaxChannelCount = 6;

        /// <summary>
        /// チャンネル数からレイアウトを取得する。対応していないチャンネル数の場合はfalseを返す
        /// </summary>
        public static bool TryFromChannelCount(int channelCount, out AudioChannelLayout layout)
        {
            switch (channelCount)
            {
                case 1:
                    layout = AudioChannelLayout.Mono;
                    return true;
                case 2:
                    layout = AudioChannelLayout.Stereo;
                    return true;
                case 6:
                    layout = AudioChannelLayout.Surround5_1;
                    return true;
                default:
                    layout = AudioChannelLayout.Stereo;
                    return false;
            }
        }

        /// <summary>
        /// チャンネル数からレイアウトを取得する。対応していないチャンネル数の場合は例外を投げる
        /// </summary>
        public static AudioChannelLayout FromChannelCount(int channelCount)
        {
            if (!TryFromChannelCount(channelCount, out AudioChannelLayout layout))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(channelCount),
                    channelCount,
                    "対応していないチャンネル数です。1、2、6のいずれかを指定してください。");
            }

            return layout;
        }

        /// <summary>
        /// 保存名からレイアウトを取得する
        /// </summary>
        public static bool TryParse(string? name, out AudioChannelLayout layout)
        {
            switch (name?.Trim().ToUpperInvariant())
            {
                case "MONO":
                    layout = AudioChannelLayout.Mono;
                    return true;
                case "STEREO":
                    layout = AudioChannelLayout.Stereo;
                    return true;
                case "SURROUND5_1":
                case "SURROUND5.1":
                case "5.1":
                    layout = AudioChannelLayout.Surround5_1;
                    return true;
                default:
                    layout = AudioChannelLayout.Stereo;
                    return false;
            }
        }

        /// <summary>
        /// 保存に使用するレイアウト名を返す
        /// </summary>
        public static string ToName(AudioChannelLayout layout)
        {
            return layout switch
            {
                AudioChannelLayout.Mono => "Mono",
                AudioChannelLayout.Stereo => "Stereo",
                AudioChannelLayout.Surround5_1 => "Surround5_1",
                _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, "未定義のチャンネルレイアウトです。"),
            };
        }
    }
}
