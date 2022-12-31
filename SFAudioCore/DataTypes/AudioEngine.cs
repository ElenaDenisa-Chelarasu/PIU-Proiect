using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

// A sample can be composed of multiple values

public class AudioEngine
{
    private const int Channels = 2;
    private const int BufferSampleSize = 512;

    private readonly AudioStream _stream;
    private readonly List<AudioInstance> _audio = new();

    public AudioEngine()
    {
        _stream = new AudioStream(this);
    }

    public TimeSpan Duration => TimeSpan.FromSeconds(SampleCount / (float)SampleRate);
    public int SampleCount { get; private set; }

    public int SampleRate => (int)_stream.SampleRate;

    public bool Loop
    {
        get => _stream.Loop;
        set
        {
            _stream.Loop = value;
            StateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Play() => _stream.Play();
    public void Pause() => _stream.Pause();
    public void Stop() => _stream.Stop();

    public TimeSpan TimePosition
    {
        get => TimeSpan.FromSeconds(_stream.SamplePosition / (float)SampleRate);
        set
        {
            if (value < TimeSpan.Zero)
                value = TimeSpan.Zero;

            if (value >= Duration)
                value = Duration;

            _stream.PlayingOffset = Time.FromSeconds((float)value.TotalSeconds);
        }
    }

    public void SetAudio(IEnumerable<AudioInstance> samples)
    {
        if (_stream.Status == SoundStatus.Playing)
            throw new InvalidOperationException("Cannot update samples while playing.");

        if (samples.Any(x => x.Channels > 2 || x.SampleRate != SampleRate))
            throw new InvalidOperationException("Unsupported audio.");

        lock (_audio)
        {
            _audio.Clear();
            _audio.AddRange(samples);

            if (_audio.Count == 0)
            {
                Stop();
                SampleCount = 0;
            }
            else
            {
                SampleCount = _audio.Select(x => x.SampleCount).Max();
            }
        }

        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void AddAudio(AudioInstance sample)
    {
        if (_stream.Status == SoundStatus.Playing)
            throw new InvalidOperationException("Cannot update samples while playing.");

        if (sample.Channels > 2 || sample.SampleRate != SampleRate)
            throw new InvalidOperationException("Unsupported audio.");

        lock (_audio)
        {
            _audio.Add(sample);

            SampleCount = _audio.Select(x => x.SampleCount).Max();
        }

        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveAudio(AudioInstance sample)
    {
        lock (_audio)
        {
            _audio.Remove(sample);

            if (_audio.Count == 0)
            {
                Stop();
                SampleCount = 0;
            }
            else
            {
                SampleCount = _audio.Select(x => x.SampleCount).Max();
            }
        }

        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    public ICollection<AudioInstance> AudioList => _audio;

    public event EventHandler? StateUpdated;

    private class AudioStream : SoundStream
    {
        private readonly AudioEngine _engine;
        private float[]? _floatData;

        private int _samplePosition;
        public int SamplePosition
        {
            get => _samplePosition;
            set
            {
                if (value > _engine.SampleCount)
                    value = _engine.SampleCount;

                _samplePosition = value;

                PlayingOffset = Time.FromSeconds(SamplePosition / (float)SampleRate);
            }
        }

        public AudioStream(AudioEngine engine)
        {
            _engine = engine;
            Initialize(2, 44100);
            RelativeToListener = true;  // Disable effect of SoundListener
        }

        protected override bool OnGetData(out short[] data)
        {
            if (SamplePosition >= _engine.SampleCount)
            {
                data = Array.Empty<short>();
                return false;
            }

            int playingOffset = (int)SamplePosition;
            int samples = Math.Min(BufferSampleSize, _engine.SampleCount - playingOffset);

            data = new short[samples * Channels];

            if (_floatData == null || _floatData.Length != data.Length)
            {
                // Recreate buffer since it isn't compatible with current samples desired
                _floatData = new float[data.Length];
            }
            else
            {
                // Zero out buffer for reuse
                Array.Fill(_floatData, 0f);
            }

            _engine.RenderInto((int)SamplePosition, (int)(SamplePosition + samples), _floatData);

            // Convert float samples back into short samples

            AudioConvert.ConvertFloatTo16(_floatData, data);

            _samplePosition += samples;
            _engine.StateUpdated?.Invoke(this, EventArgs.Empty);

            return true;
        }

        protected override void OnSeek(Time timeOffset)
        {
            _samplePosition = (int)Math.Round(timeOffset.AsSeconds() * SampleRate);
            _engine.StateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    public float[] Render(TimeSpan start, TimeSpan end)
    {
        int sampleStart = (int)(start.TotalSeconds * SampleRate);
        int sampleEnd = (int)(end.TotalSeconds * SampleRate);

        var data = new float[(sampleEnd - sampleStart) * Channels];

        RenderInto(sampleStart, sampleEnd, data);

        return data;
    }

    public void RenderInto(int sampleStart, int sampleEnd, float[] floatData)
    {
        if (floatData.Length / Channels != sampleEnd - sampleStart)
        {
            throw new InvalidOperationException("Badly sized float data.");
        }

        if (sampleStart >= SampleCount)
        {
            return;
        }

        if (sampleStart < 0 || sampleStart > sampleEnd)
        {
            throw new InvalidOperationException("Invalid args");
        }

        if (sampleEnd >= SampleCount)
        {
            sampleEnd = SampleCount;
        }

        int playingOffset = sampleStart;
        int samples = sampleEnd - sampleStart;

        int fillStart = playingOffset * Channels;
        int fillEnd = (playingOffset + samples) * Channels;

        // Mix some shit
        foreach (var audio in _audio)
        {
            // We can only mix in samples that are shared by both intervals

            int audioStart = 0;
            int audioEnd = audio.SampleCount * Channels;

            int mixStart = Math.Max(fillStart, audioStart);
            int mixEnd = Math.Min(fillEnd, audioEnd);

            if (mixStart >= mixEnd)
            {
                // This audio doesn't play in this interval
                continue;
            }

            int trueFillStart = mixStart - fillStart;
            int trueAudioStart = mixStart - audioStart;
            int dataCount = mixEnd - mixStart;

            float globalVolumeMod = Math.Clamp(1f, 0f, 1f); // No modifiers implemented yet

            float leftVolumeMod = audio.LeftMuted ? 0f : Math.Clamp(audio.LeftVolume, 0f, 1f) * globalVolumeMod;
            float rightVolumeMod = audio.RightMuted ? 0f : Math.Clamp(audio.RightVolume, 0f, 1f) * globalVolumeMod;

            // Stereo mixing
            if (audio.Channels == 2)
            {
                // There are two values for each sample

                unsafe
                {
                    fixed (float* fPtr = &floatData[trueFillStart])
                    {
                        fixed (float* aPtr = &audio.Data.Span[trueAudioStart])
                        {
                            int dataIndex = 0;
                            while (dataIndex < dataCount)
                            {
                                *(fPtr + dataIndex) += *(aPtr + dataIndex) * leftVolumeMod;
                                *(fPtr + dataIndex + 1) += *(aPtr + dataIndex + 1) * rightVolumeMod;
                                dataIndex += 2;
                            }
                        }
                    }
                }
            }

            // Mono mixing - duplicate samples to simualte stereo
            else if (audio.Channels == 1)
            {
                // There is one value for each sample which we must duplicate
                // Advance half as fast on audio source

                unsafe
                {
                    fixed (float* fPtr = &floatData[trueFillStart])
                    {
                        fixed (float* aPtr = &audio.Data.Span[trueAudioStart / 2])
                        {
                            int dataIndex = 0;
                            int sampleIndex = 0;
                            while (dataIndex < dataCount)
                            {
                                float value = *(aPtr + sampleIndex);

                                *(fPtr + dataIndex) += value * leftVolumeMod;
                                *(fPtr + dataIndex) += value * rightVolumeMod;

                                dataIndex += 2;
                                sampleIndex += 1;
                            }
                        }
                    }
                }
            }
        }

        return;
    }
}
