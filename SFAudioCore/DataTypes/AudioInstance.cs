using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioInstance
{
    public AudioInstance(Audio source, int sampleStart)
    {
        Source = source;
        SampleStart = sampleStart;
    }

    public AudioInstance(Audio source, TimeSpan timeStart)
    {
        Source = source;
        SampleStart = (int)(timeStart.TotalSeconds * source.SampleRate);
    }

    public float Panning { get; set; } = 0f;

    public float Volume { get; set; } = 1f;

    public Audio Source { get; set; }

    public int SampleStart { get; set; }
}
