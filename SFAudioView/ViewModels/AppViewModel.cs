using Prism.Commands;
using SFAudioCore.DataTypes;
using SFAudioView.GUI;
using SFML.Audio;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SFAudioView.ViewModels;

/// <summary>
/// Contains business logic for main window.
/// </summary>
public class AppViewModel : ViewModelBase
{
    public DelegateCommand<RoutedEventArgs> PlayCommand { get; }
    public DelegateCommand<RoutedEventArgs> PauseCommand { get; }
    public DelegateCommand<RoutedEventArgs> StopCommand { get; }
    public DelegateCommand<RoutedEventArgs> SkipLeftCommand { get; }
    public DelegateCommand<RoutedEventArgs> SkipRightCommand { get; }
    public DelegateCommand<RoutedEventArgs> ToggleCommand { get; }
    public DelegateCommand<WrappedValueEvent<string>> FileOpenCommand { get; }
    public DelegateCommand<WrappedValueEvent<string>> FileSaveCommand { get; }
    public DelegateCommand<WrappedValueEvent<AudioInstance>> TrackRemovedCommand { get; }

    // The wrapped model
    private AudioEngine Engine { get; } = new();

    public AppViewModel()
    {
        Engine.StateUpdated += (object? sender, EventArgs e) =>
        {
            Notify(nameof(SampleRateText));
            Notify(nameof(TimeCursorText));
            Notify(nameof(DurationText));
            Notify(nameof(LoopingText));
            Notify(nameof(PlayPosition));
        };

        PlayCommand = new((e) => Play());
        PauseCommand = new((e) => Pause());
        StopCommand = new((e) => Stop());
        SkipLeftCommand = new((e) => SkipLeft());
        SkipRightCommand = new((e) => SkipRight());
        ToggleCommand = new((e) => ToggleLoop());
        FileOpenCommand = new((e) => LoadNewTrack(e.Value));
        FileSaveCommand = new((e) => SaveAudio(e.Value));
        TrackRemovedCommand = new((e) => RemoveAudioTrack(e.Value));
    }

    public ObservableCollection<AudioInstance> AudioTracks { get; } = new();

    private string _actionDescriptionText = "";
    public string ActionDescriptionText
    {
        get => _actionDescriptionText;
        set { _actionDescriptionText = value; Notify(); }
    }

    private TimeSpan _playRegionSize;
    public TimeSpan PlayRegionSize
    {
        get => _playRegionSize;
        set { _playRegionSize = value; Notify(); }
    }

    private TimeSpan _playRegionStart;
    public TimeSpan PlayRegionStart
    {
        get => _playRegionStart;
        set { _playRegionStart = value; Notify(); }
    }

    public TimeSpan PlayPosition
    {
        get => Engine.TimePosition;
        set { Engine.TimePosition = value; Notify(); }
    }

    public void Play() => Engine.Play();
    public void Pause() => Engine.Pause();
    public void Stop() => Engine.Stop();
    public void SkipLeft() => Engine.TimePosition -= GetSkipTimeAmount();
    public void SkipRight() => Engine.TimePosition += GetSkipTimeAmount();
    public void ToggleLoop() => Engine.Loop = !Engine.Loop;

    public void SaveAudio(string path)
    {
        var temp_samples = Engine.Render(TimeSpan.Zero, Engine.Duration);

        var samples = new short[temp_samples.Length];
        AudioConvert.ConvertFloatTo16(temp_samples, samples);

        using var buffer = new SoundBuffer(samples, 2, 44100);

        buffer.SaveToFile(path);
    }

    public void LoadNewTrack(string path)
    {
        Engine.Stop();

        var audio = AudioProvider.LoadFromFile(path);

        Engine.AddAudio(audio);
        AudioTracks.Add(audio);
        Notify(nameof(RenderStart));
        Notify(nameof(RenderDuration));
    }

    public void RemoveAudioTrack(AudioInstance audio)
    {
        Engine.RemoveAudio(audio);
        AudioTracks.Remove(audio);

        Notify(nameof(RenderStart));
        Notify(nameof(RenderDuration));
    }

    public TimeSpan TotalDuration => Engine.Duration;

    public string SampleRateText => Engine.SampleRate + " Hz";
    public string TimeCursorText => Engine.TimePosition.ToString("hh\\:mm\\:ss\\.fff");
    public string DurationText => Engine.Duration.ToString("hh\\:mm\\:ss\\.fff");
    public string LoopingText => Engine.Loop ? "Looping On." : "Looping Off.";

    public static TimeSpan GetSkipTimeAmount()
    {
        TimeSpan baseTime = TimeSpan.FromSeconds(5);

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            return baseTime * 60;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            return baseTime * 12;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            return baseTime * 2;

        return baseTime;
    }

    public double ScrollZoom
    {
        get => _scrollZoom;
        set 
        { 
            _scrollZoom = Math.Clamp(value, 1f, 5f); 
            Notify();
            Notify(nameof(RenderStart));
            Notify(nameof(RenderDuration));
        }
    }
    private double _scrollZoom = 3.5;

    public double ScrollPosition
    {
        get => _scrollPosition;
        set
        {
            _scrollPosition = Math.Clamp(value, 0f, 1f);
            Notify();
            Notify(nameof(RenderStart));
            Notify(nameof(RenderDuration));
        }
    }
    private double _scrollPosition = 0;


    public TimeSpan RenderStart => (Engine.Duration - RenderDuration) * _scrollPosition;
    public TimeSpan RenderDuration
    {
        get
        {
            TimeSpan baseValue = _scrollZoom switch
            {
                < 1 => TimeSpan.FromMilliseconds(500),
                >= 1 and < 2 => TimeSpan.FromMilliseconds(500 + 499 * (_scrollZoom - 1)),
                >= 2 and < 3 => TimeSpan.FromSeconds(1 + 59 * (_scrollZoom - 2)),
                >= 3 and < 4 => TimeSpan.FromMinutes(1 + 59 * (_scrollZoom - 3)),
                >= 4 => TimeSpan.FromHours(1),
                _ => TimeSpan.FromSeconds(1)
            };

            return TimeSpan.FromSeconds(Math.Clamp(baseValue.TotalSeconds, 0, Engine.Duration.TotalSeconds));
        }
    }
}
