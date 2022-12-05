using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public class AudioEngine : IDisposable
{
    private readonly Thread _runner;
    private bool _active = true;
    private readonly object _lock = new();
    private readonly List<AudioSample> _samples = new();
    private readonly Dictionary<AudioSample, Sound> _players = new();

    public AudioEngine()
    {
        _runner = new Thread(ThreadLoop);
        _runner.Start();
    }

    private TimeSpan _timeCursor = TimeSpan.Zero;
    public TimeSpan TimeCursor
    {
        get => _timeCursor;
        set => SetupPlayback(timeCursor: value);
    }

    private bool _playing = false;
    public bool Playing
    {
        get => _playing;
        set => SetupPlayback(playing: value);
    }

    private bool _looping = false;
    public bool Looping
    {
        get => _looping;
        set => SetupPlayback(looping: value);
    }

    public int SampleCount => _samples.Count;

    public TimeSpan ProjectLength { get; internal set; }

    private void ThreadLoop()
    {
        var clock = new Clock();

        while (_active)
        {
            Time elapsed = clock.Restart();
            Thread.Sleep(1);

            UpdatePlayers();

            if (_playing)
            {
                lock (_lock)
                {
                    _timeCursor += TimeSpan.FromSeconds(elapsed.AsSeconds());
                    if (_timeCursor >= ProjectLength)
                    {
                        if (_looping)
                        {
                            SetupPlayback(playing: false, timeCursor: TimeSpan.Zero);
                        }
                        else
                        {
                            _timeCursor -= ProjectLength;
                        }
                    }
                }
            }

            EngineTick?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateSamples(IEnumerable<AudioSample> samples)
    {
        lock (_lock)
        {
            _samples.Clear();
            _samples.AddRange(samples);

            foreach ((var _, var sample) in _players)
            {
                sample.Stop();
                sample.Dispose();
            }

            _players.Clear();

            TimeSpan projectLength = TimeSpan.Zero;

            foreach (var sample in _samples)
            {
                var duration = sample.Start + sample.Source.Duration;

                if (projectLength < duration)
                    projectLength = duration;
            }

            ProjectLength = projectLength;
        }
    }

    private void UpdatePlayers()
    {
        lock (_lock)
        {
            if (!_playing)
            {
                foreach ((var _, var sound) in _players)
                {
                    sound.Stop();
                    sound.Dispose();
                }

                _players.Clear();
                return;
            }

            var shouldBePlaying = _samples.Where(x => x.Start <= TimeCursor && TimeCursor <= x.Start + x.Source.Duration).ToList();

            var shouldRemove = _players.Keys.Where(x => !shouldBePlaying.Contains(x)).ToList();
            var shouldAdd = shouldBePlaying.Where(x => !_players.ContainsKey(x)).ToList();

            foreach (var sample in shouldRemove)
            {
                _players[sample].Stop();
                _players[sample].Dispose();
                _players.Remove(sample);
            }

            foreach (var sample in shouldAdd)
            {
                _players[sample] = new Sound(sample.Source.MakeBuffer())
                {
                    PlayingOffset = Time.FromSeconds((float)(_timeCursor - sample.Start).TotalSeconds),
                    Loop = false  // No point in looping fragments of a track
                };

                _players[sample].Play();
            }

            // Check for desync
            foreach ((var sample, var sound) in _players)
            {
                TimeSpan playingOffset = TimeSpan.FromSeconds(sound.PlayingOffset.AsSeconds());

                var delta = TimeCursor - (playingOffset + sample.Start);

                if (delta >= TimeSpan.FromMilliseconds(1))
                {
                    // Update positions
                    sound.PlayingOffset = Time.FromSeconds((float)(TimeCursor - sample.Start).TotalSeconds);
                }
            }
        }
    }

    private void SetupPlayback(
        bool? playing = null,
        bool? looping = null,
        TimeSpan? timeCursor = null
        )
    {
        lock (_lock)
        {
            _playing = playing.GetValueOrDefault(_playing);
            _looping = looping.GetValueOrDefault(_looping);
            _timeCursor = timeCursor.GetValueOrDefault(_timeCursor);

            if (_playing)
            {
                foreach ((var _, var sound) in _players)
                {
                    if (sound.Status != SoundStatus.Playing)
                        sound.Play();
                }
            }
            else
            {
                foreach ((var _, var sound) in _players)
                {
                    if (sound.Status != SoundStatus.Paused)
                        sound.Pause();
                }
            }
        }

        PlaybackChanged?.Invoke(this, new PlaybackChangedArgs()
        {
            Looping = _looping,
            Playing = _playing,
            TimeCursor = _timeCursor
        });
    }

    public class PlaybackChangedArgs
    {
        public bool Playing { get; set; }
        public bool Looping { get; set; }
        public TimeSpan TimeCursor { get; set; }
    }

    public event EventHandler<PlaybackChangedArgs>? PlaybackChanged;

    public event EventHandler? EngineTick;

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

    ~AudioEngine()
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
