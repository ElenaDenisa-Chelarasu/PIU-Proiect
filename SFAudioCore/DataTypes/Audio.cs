using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;

namespace SFAudioCore.DataTypes;

public class Audio
{
    public uint Channels { get; set; } = 1;

    public float[] Samples { get; set; } = Array.Empty<float>();

    public static Audio LoadFromFile(string fileName)
    {
        var buffer = new SoundBuffer(fileName);

        return new Audio
        {
            Channels = buffer.ChannelCount,
            Samples = buffer.Samples.Select(x => x / (float)short.MaxValue).ToArray()
        };
    }

    public SoundBuffer MakeBuffer()
    {
        return new SoundBuffer(Samples.Select(x => (short)(x * short.MaxValue)).ToArray(), Channels, 44100); 
    }
}
