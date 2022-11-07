namespace SFAudioCore.DataTypes;

public class AudioTrack
{
    public AudioChannel Left { get; set; } = new();

    public AudioChannel? Right { get; set; }

    public bool IsMono => Right == null;
    public bool IsStereo => Right != null;
}