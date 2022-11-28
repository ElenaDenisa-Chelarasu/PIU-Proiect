using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioProject : IDisposable
{
    private readonly Thread _runner;
    private bool _active = true;
    private readonly object _lock = new();
    private List<Audio> _audioTracks = new();
    private List<Sound> _sounds = new();

    private TimeSpan _timeCursor = TimeSpan.Zero;
    public TimeSpan TimeCursor
    {
        get => _timeCursor;
        set
        {
            lock (_lock)
            {
                SetupPlayback(_playing, value);
            }
        }
    }

    private bool _playing = false;
    public bool Playing
    {
        get => _playing;
        set 
        {
            lock (_lock)
            {
                SetupPlayback(value, _timeCursor);
            }
        }
    }

    public AudioProject()
    {
        _runner = new Thread(ThreadLoop);
    }

    private void ThreadLoop()
    {
        while (_active)
        {
            Thread.Sleep(1);
            //Thread.Yield();
        }
    }

    private void SetupPlayback(bool playing, TimeSpan timeCursor)
    {
        _playing = playing;
        _timeCursor = timeCursor;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_active)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _active = false;
        }
    }

    ~AudioProject()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
