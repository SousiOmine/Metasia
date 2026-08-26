namespace Metasia.Core.Sounds;

public interface IAudioChannelLayout : IEquatable<IAudioChannelLayout>
{
    /// <summary>
    /// 格納順は必ずビット昇順になる
    /// </summary>
    IReadOnlyList<SpeakerPosition> Channels { get; }
    
    int ChannelCount { get; }

    /// <summary>
    /// 指定したスピーカーが配置に含まれているか判定する
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    bool Contains(SpeakerPosition position);
}

public enum SpeakerPosition : ulong
{
    FL  = 1UL << 0,  // FRONT LEFT
    FR  = 1UL << 1,  // FRONT RIGHT
    FC  = 1UL << 2,  // FRONT CENTER
    LFE = 1UL << 3,  // LOW FREQUENCY EFFECT
    BL  = 1UL << 4,  // BACK LEFT
    BR  = 1UL << 5,  // BACK RIGHT
    FLC = 1UL << 6,  // FRONT LEFT OF CENTER
    FRC = 1UL << 7,  // FRONT RIGHT OF CENTER
    BC  = 1UL << 8,  // BACK CENTER
    SL  = 1UL << 9,  // SIDE LEFT
    SR  = 1UL << 10, // SIDE RIGHT
    TC  = 1UL << 11, // TOP CENTER
    TFL = 1UL << 12, // TOP FRONT LEFT
    TFC = 1UL << 13, // TOP FRONT CENTER
    TFR = 1UL << 14, // TOP FRONT RIGHT
    TBL = 1UL << 15, // TOP BACK LEFT
    TBC = 1UL << 16, // TOP BACK CENTER
    TBR = 1UL << 17, // TOP BACK RIGHT
}