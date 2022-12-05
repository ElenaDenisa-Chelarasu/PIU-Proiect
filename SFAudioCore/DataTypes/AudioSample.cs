using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioSample
{
    public AudioSample(Audio source, TimeSpan start)
    {
        Source = source;
        Start = start;
    }

    public Audio Source { get; set; }

    public TimeSpan Start { get; set; }
}
