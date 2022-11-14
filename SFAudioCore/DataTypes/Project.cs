using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioProject
{
    public Dictionary<string, AudioTrack> AudioTracks { get; } = new();

    //esantionarea unui fisier audio
}
