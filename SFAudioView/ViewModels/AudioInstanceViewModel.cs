using SFAudioCore.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioView.ViewModels;

public class AudioInstanceViewModel : ViewModelBase
{
    private AudioInstance? _audio;
    public AudioInstance Audio
    {
        get => _audio ?? throw new NullReferenceException("Audio has not been set");
        set => _audio = value;
    }

    public double LeftChannelVolume
    {
        get => Audio.LeftVolume;
        set { Audio.LeftVolume = (float)value; Notify(); }
    }

    public double RightChannelVolume
    {
        get => Audio.RightVolume;
        set { Audio.RightVolume = (float)value; Notify(); }
    }

    public bool LeftChannelMuted
    {
        get => Audio.LeftMuted;
        set { Audio.LeftMuted = value; Notify(); }
    }

    public bool RightChannelMuted
    {
        get => Audio.RightMuted;
        set { Audio.RightMuted = value; Notify(); }
    }
}
