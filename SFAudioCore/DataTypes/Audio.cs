using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;

namespace SFAudioCore.DataTypes;

public class Audio
{
    public int Channels { get; private set; } = 1;

    /// <summary>
    /// Values for all of the samples in the audio.
    /// </summary>
    public float[] Data { get; private set; } = Array.Empty<float>();

    public int SampleCount => (Data.Length / Channels);

    public int SampleRate { get; private set; } = 44100;

    public TimeSpan Duration => TimeSpan.FromSeconds(SampleCount / (float)SampleRate);

    public static Audio LoadFromFile(string fileName)
    {
        var buffer = new SoundBuffer(fileName);
        var data = new float[buffer.Samples.Length];

        AudioConvert.Convert16ToFloat(buffer.Samples, data);

        return new Audio
        {
            Channels = (int)buffer.ChannelCount,
            Data = data,
            SampleRate = (int)buffer.SampleRate
        };
    }

    public static Audio WhiteNoise(int channels, int sampleRate, TimeSpan duration)
        => WhiteNoise(channels, sampleRate, (int)(duration.TotalSeconds * sampleRate));

    public static Audio WhiteNoise(int channels, int sampleRate, int samples)
    {
        var random = new Random();

        return new Audio
        {
            Channels = channels,
            SampleRate = sampleRate,
            Data = Enumerable.Range(0, (int)samples).Select(x => Math.Clamp((random.NextSingle() - 0.5f) * 2, -1f, 1f)).ToArray()
        };
    }

    public static Audio Silence(int channels, int sampleRate, TimeSpan duration)
        => Silence(channels, sampleRate, (int)(duration.TotalSeconds * sampleRate));

    public static Audio Silence(int channels, int sampleRate, int samples)
    {
        return new Audio
        {
            Channels = channels,
            SampleRate = sampleRate,
            Data = new float[samples]
        };
    }

    public SoundBuffer MakeBuffer()
    {
        short[] data = new short[Data.Length];
        AudioConvert.ConvertFloatTo16(Data, data);

        return new SoundBuffer(data, (uint)Channels, (uint)SampleRate); 
    }
}
