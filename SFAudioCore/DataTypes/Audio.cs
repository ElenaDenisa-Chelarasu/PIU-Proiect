using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;

namespace SFAudioCore.DataTypes;

public class Audio
{
    public uint Channels { get; private set; } = 1;

    /// <summary>
    /// Values for all of the samples in the audio.
    /// </summary>
    public float[] Data { get; private set; } = Array.Empty<float>();

    public uint SampleCount => (uint)(Data.Length / Channels);

    public uint SampleRate { get; private set; } = 44100;

    public TimeSpan Duration => TimeSpan.FromSeconds(SampleCount / (float)SampleRate);

    public static Audio LoadFromFile(string fileName)
    {
        var buffer = new SoundBuffer(fileName);
        var data = new float[buffer.Samples.Length];

        AudioConvert.Convert16ToFloat(buffer.Samples, data);

        return new Audio
        {
            Channels = buffer.ChannelCount,
            Data = data,
            SampleRate = buffer.SampleRate
        };
    }

    public SoundBuffer MakeBuffer()
    {
        short[] data = new short[Data.Length];
        AudioConvert.ConvertFloatTo16(Data, data);

        return new SoundBuffer(data, Channels, SampleRate); 
    }
}
