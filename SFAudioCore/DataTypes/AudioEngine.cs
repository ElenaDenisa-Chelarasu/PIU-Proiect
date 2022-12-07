using SFML.Audio;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

// A sample can be composed of multiple values

public class AudioEngine
{
    private readonly AudioStream _stream;
    private readonly List<AudioInstance> _audio = new();

    public AudioEngine()
    {
        _stream = new AudioStream(this);
    }

    public TimeSpan Duration => TimeSpan.FromSeconds(SampleCount / (float)SampleRate);
    public uint SampleCount { get; private set; }

    public uint SampleRate => _stream.SampleRate;

    public bool Loop
    {
        get => _stream.Loop;
        set => _stream.Loop = value;
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

        if (samples.Any(x => x.Source.Channels > 2 || x.Source.SampleRate != SampleRate))
            throw new InvalidOperationException("Unsupported audio.");

        lock (_audio)
        {
            _audio.Clear();
            _audio.AddRange(samples);

            SampleCount = _audio.Select(x => x.SampleStart + x.Source.SampleCount).Max();
        }

        StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? StateUpdated;

    private class AudioStream : SoundStream
    {
        private const uint Channels = 2;
        private const uint BufferSampleSize = 512;

        private readonly AudioEngine _engine;
        private float[]? _floatData;

        private uint _samplePosition;
        public uint SamplePosition
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

            uint playingOffset = SamplePosition;
            uint samples = Math.Min(BufferSampleSize, _engine.SampleCount - playingOffset);

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

            uint fillStart = playingOffset * Channels;
            uint fillEnd = (playingOffset + samples) * Channels;

            // Mix some shit
            foreach (var audio in _engine._audio)
            {
                // We can only mix in samples that are shared by both intervals

                uint audioStart = audio.SampleStart * Channels;
                uint audioEnd = (audio.SampleStart + audio.Source.SampleCount) * Channels;

                uint mixStart = Math.Max(fillStart, audioStart);
                uint mixEnd = Math.Min(fillEnd, audioEnd);

                if (mixStart >= mixEnd)
                {
                    // This audio doesn't play in this interval
                    continue;
                }

                uint trueFillStart = mixStart - fillStart;
                uint trueAudioStart = mixStart - audioStart;
                uint dataCount = mixEnd - mixStart;

                float globalVolumeMod = Math.Clamp(audio.Volume, 0f, 1f);

                // Stereo mixing
                if (audio.Source.Channels == 2)
                {
                    // There are two values for each sample
                    // -1f = full left, 1f = full right
                    float leftVolumeMod = Math.Clamp(-(audio.Panning - 1f), 0f, 1f) * globalVolumeMod;
                    float rightVolumeMod = Math.Clamp(audio.Panning + 1f, 0f, 1f) * globalVolumeMod;

                    unsafe
                    {
                        fixed (float* fPtr = &_floatData[trueFillStart])
                        {
                            fixed (float* aPtr = &audio.Source.Data[trueAudioStart])
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
                else if (audio.Source.Channels == 1)
                {
                    // There is one value for each sample which we must duplicate
                    // Advance half as fast on audio source

                    unsafe
                    {
                        fixed (float* fPtr = &_floatData[trueFillStart])
                        {
                            fixed (float* aPtr = &audio.Source.Data[trueAudioStart / 2])
                            {
                                int dataIndex = 0;
                                int sampleIndex = 0;
                                while (dataIndex < dataCount)
                                {
                                    float value = *(aPtr + sampleIndex) * globalVolumeMod;

                                    *(fPtr + dataIndex) += value;
                                    *(fPtr + dataIndex) += value;

                                    dataIndex += 2;
                                    sampleIndex += 1;
                                }
                            }
                        }
                    }
                }
            }

            // Convert float samples back into short samples

            AudioConvert.ConvertFloatTo16(_floatData, data);

            _samplePosition += samples;
            _engine.StateUpdated?.Invoke(this, EventArgs.Empty);

            return true;
        }

        protected override void OnSeek(Time timeOffset)
        {
            _samplePosition = (uint)Math.Round(timeOffset.AsSeconds() * SampleRate);
            _engine.StateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

}
