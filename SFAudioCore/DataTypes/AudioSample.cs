using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioSample
{
    public Memory<float> Data { get; set; } = Array.Empty<float>();
}
