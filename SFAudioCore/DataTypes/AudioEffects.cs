using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public static class AudioEffects
{
    public static void Amplify(AudioInstance audio, int sampleStart, int sampleEnd, float amp, int? channel)
    {
        if (sampleStart > sampleEnd)
            throw new ArgumentException("Invalid args");

        var copy = audio.Data.ToArray();

        sampleStart = Math.Clamp(sampleStart, 0, audio.SampleCount) * audio.Channels;
        sampleEnd = Math.Clamp(sampleEnd, 0, audio.SampleCount) * audio.Channels;

        int size = copy.Length;
        int channels = audio.Channels;

        unsafe
        {
            if (channel == null)
            {
                fixed (float* ptr = copy)
                {
                    for (int i = sampleStart; i < sampleEnd; i++)
                    {
                        *(ptr + i) = Math.Clamp(*(ptr + i) * amp, -1f, 1f);
                    }
                }
            }
            else
            {
                fixed (float* ptr = copy)
                {
                    for (int i = sampleStart + channel.Value; i < sampleEnd; i += channels)
                    {
                        *(ptr + i) = Math.Clamp(*(ptr + i) * amp, -1f, 1f);
                    }
                }
            }
            
        }
        

        audio.Modify(copy);
    }
}
