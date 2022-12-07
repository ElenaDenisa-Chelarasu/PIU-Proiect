using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioInstance
{
    public AudioInstance(Audio source, uint sampleStart)
    {
        Source = source;
        SampleStart = sampleStart;
    }

    public AudioInstance(Audio source, TimeSpan timeStart)
    {
        Source = source;
        SampleStart = (uint)(timeStart.TotalSeconds * source.SampleRate);
    }

    public float Panning { get; set; } = 0f;

    public float Volume { get; set; } = 1f;

    public Audio Source { get; set; }

    public uint SampleStart { get; set; }
}
