namespace SFAudioCore.DataTypes;

public class IntAudio
{
    public Memory<uint> Data { get; set; } = Array.Empty<uint>();
}

public class FloatAudio
{
    public Memory<float> Data { get; set; } = Array.Empty<float>();
}