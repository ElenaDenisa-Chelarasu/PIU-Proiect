using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public static class AudioProvider
{
    public static AudioInstance LoadFromFile(string fileName)
    {
        var buffer = new SoundBuffer(fileName);
        var data = new float[buffer.Samples.Length];

        AudioConvert.Convert16ToFloat(buffer.Samples, data);

        return new AudioInstance(data, (int)buffer.ChannelCount, (int)buffer.SampleRate)
        {
            Name = Path.GetFileNameWithoutExtension(fileName)
        };
    }

    public static AudioInstance WhiteNoise(int channels, int sampleRate, TimeSpan duration)
        => WhiteNoise(channels, sampleRate, (int)(duration.TotalSeconds * sampleRate));

    public static AudioInstance WhiteNoise(int channels, int sampleRate, int samples)
    {
        var random = new Random();

        var data = Enumerable.Range(0, samples).Select(x => Math.Clamp((random.NextSingle() - 0.5f) * 2, -1f, 1f)).ToArray();

        return new AudioInstance(data, channels, sampleRate);
    }

    public static AudioInstance Silence(int channels, int sampleRate, TimeSpan duration)
        => Silence(channels, sampleRate, (int)(duration.TotalSeconds * sampleRate));

    public static AudioInstance Silence(int channels, int sampleRate, int samples)
    {
        return new AudioInstance(new float[samples], channels, sampleRate);
    }
}
