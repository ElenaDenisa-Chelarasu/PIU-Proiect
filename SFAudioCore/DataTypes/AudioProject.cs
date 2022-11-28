using SFML.Audio;
using SFML.System;
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
    private readonly List<Audio> _audioTracks = new();
    private readonly List<Sound> _sounds = new();

    public AudioProject(IEnumerable<Audio> audio)
        : this()
    {
        _audioTracks.AddRange(audio);
        _sounds.AddRange(_audioTracks.Select(x => new Sound(x.MakeBuffer())));
        _sounds.ForEach(x => x.Pause());
        ProjectLength = TimeSpan.FromSeconds(_audioTracks.Select(x => x.Samples.Length / 44100.0).Max());
    }

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

    public TimeSpan ProjectLength { get; private set; }

    public AudioProject()
    {
        _runner = new Thread(ThreadLoop);
        _runner.Start();
    }

    private void ThreadLoop()
    {
        Time last = Time.Zero;

        var clock = new Clock();

        while (_active)
        {
            last = clock.Restart();
            Thread.Sleep(1);

            if (_playing)
            {
                lock (_lock)
                {
                    _timeCursor += TimeSpan.FromSeconds(last.AsSeconds());
                    if (_timeCursor >= ProjectLength)
                    {
                        _timeCursor -= ProjectLength;
                    }
                }
            }
        }
    }

    private void SetupPlayback(bool playing, TimeSpan timeCursor)
    {
        _playing = playing;
        _timeCursor = timeCursor;

        foreach (var x in _sounds)
        {
            x.Loop = true;
            x.PlayingOffset = Time.FromSeconds((float)_timeCursor.TotalSeconds);
        }

        if (_playing)
        {
            foreach (var x in _sounds)
            {
                if (x.Status != SoundStatus.Playing)
                    x.Play();
            }
        }
        else
        {
            foreach (var x in _sounds)
            {
                if (x.Status != SoundStatus.Paused)
                    x.Pause();
            }
        }
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
