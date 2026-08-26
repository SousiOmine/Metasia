namespace Metasia.Core.Sounds;

public sealed class ChannelLayout : IAudioChannelLayout
{
    public static readonly ChannelLayout Mono = new(SpeakerPosition.FC);

    public static readonly ChannelLayout Stereo = new(SpeakerPosition.FL, SpeakerPosition.FR);

    /// <summary>
    /// 5.1 = [FL, FR, FC, LFE, BL, BR]
    /// 後方スピーカー、FFmpeg の 5.1 の既定
    /// </summary>
    public static readonly ChannelLayout FivePointOne = new(
        SpeakerPosition.FL, SpeakerPosition.FR, SpeakerPosition.FC, SpeakerPosition.LFE,
        SpeakerPosition.BL, SpeakerPosition.BR);

    /// <summary>
    /// 5.1(side) = [FL, FR, FC, LFE, SL, SR]
    /// 側方スピーカー
    /// </summary>
    public static readonly ChannelLayout FivePointOneSide = new(
        SpeakerPosition.FL, SpeakerPosition.FR, SpeakerPosition.FC, SpeakerPosition.LFE,
        SpeakerPosition.SL, SpeakerPosition.SR);

    /// <summary>
    /// 7.1 = [FL, FR, FC, LFE, BL, BR, SL, SR]
    /// </summary>
    public static readonly ChannelLayout SevenPointOne = new(
        SpeakerPosition.FL, SpeakerPosition.FR, SpeakerPosition.FC, SpeakerPosition.LFE,
        SpeakerPosition.BL, SpeakerPosition.BR, SpeakerPosition.SL, SpeakerPosition.SR);

    public IReadOnlyList<SpeakerPosition> Channels { get; }
    
    public int ChannelCount { get; }
    
    public bool Contains(SpeakerPosition position)
    {
        return Channels.Contains(position);
    }

    public bool Equals(IAudioChannelLayout? other)
    {
        if (other is null) return false;
        return Channels.SequenceEqual(other.Channels);
    }

    public ChannelLayout(params SpeakerPosition[] channels)
    {
        Channels = channels;
        ChannelCount = channels.Length;
    }
}
