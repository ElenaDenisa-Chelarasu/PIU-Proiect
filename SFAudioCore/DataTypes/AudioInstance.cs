using SFML.Audio;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SFAudioCore.DataTypes;

public class AudioInstance : INotifyPropertyChanged
{
    public AudioInstance(ReadOnlySpan<float> data, int channels, int sampleRate)
    {
        Modify(data, channels, sampleRate);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// How many physical samples there are per logical sample.
    /// </summary>
    public int Channels { get; private set; }

    /// <summary>
    /// Raw sample data.
    /// </summary>
    public ReadOnlyMemory<float> Data => _data;
    private float[] _data = Array.Empty<float>();

    /// <summary>
    /// How many logical samples per second.
    /// </summary>
    public int SampleRate { get; private set; }

    /// <summary>
    /// A name for this audio instance.
    /// </summary>
    public string Name
    {
        get => _name;
        set { _name = value; Notify(); }
    }
    private string _name = "Unknown";

    /// <summary>
    /// Volume of left channel.
    /// </summary>
    public float LeftVolume
    {
        get => _leftVolume;
        set { _leftVolume = Math.Clamp(value, 0f, 1f); Notify(); }
    }
    private float _leftVolume = 1f;

    /// <summary>
    /// Whenever to mute the left channel entirely.
    /// </summary>
    public bool LeftMuted
    {
        get => _leftMuted;
        set { _leftMuted = value; Notify(); }
    } 
    private bool _leftMuted = false;

    /// <summary>
    /// Volume of right channel.
    /// </summary>
    public float RightVolume
    {
        get => _rightVolume;
        set { _rightVolume = Math.Clamp(value, 0f, 1f); Notify(); }
    }
    private float _rightVolume = 1f;

    /// <summary>
    /// Whenever to mute the right channel entirely.
    /// </summary>
    public bool RightMuted
    {
        get => _rightMuted;
        set { _rightMuted = value; Notify(); }
    }
    private bool _rightMuted = false;

    public TimeSpan Duration => TimeSpan.FromSeconds(Data.Length / Channels / (float)SampleRate);

    public int SampleCount => Data.Length / Channels;

    public void Modify(ReadOnlySpan<float> data, int channels, int sampleRate)
    {
        if (sampleRate != 44100)
            throw new InvalidOperationException($"Sample rate of {sampleRate} is not supported.");

        if (channels <= 0 || channels > 2)
            throw new InvalidOperationException($"Channel count of {channels} is not supported.");

        if (data.Length % channels != 0)
            throw new InvalidOperationException($"Sample data is not matched to a channel count of {channels}.");

        _data = data.ToArray();
        Channels = channels;
        SampleRate = sampleRate;

        Notify(nameof(Data));
        Notify(nameof(Channels));
        Notify(nameof(SampleRate));
        Notify(nameof(Duration));
        Notify(nameof(SampleCount));
    }

    public SoundBuffer MakeBuffer()
    {
        short[] data = new short[Data.Length];
        AudioConvert.ConvertFloatTo16(Data.Span, data);

        return new SoundBuffer(data, (uint)Channels, (uint)SampleRate);
    }

    private void Notify([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
