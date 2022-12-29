using Prism.Commands;
using SFAudioCore.DataTypes;
using SFAudioView.GUI;
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

    public short[] SaveAudio()
    {
        var temp_samples = Engine.Render(TimeSpan.Zero, Engine.Duration);

        var samples = new short[temp_samples.Length];
        AudioConvert.ConvertFloatTo16(temp_samples, samples);

        return samples;
    }

    public void LoadNewTrack(string path)
    {
        Engine.Stop();

        var audio = AudioProvider.LoadFromFile(path);

        Engine.AddAudio(audio);
        AudioTracks.Add(audio);
    }

    public void RemoveAudioTrack(AudioInstance audio)
    {
        Engine.RemoveAudio(audio);
        AudioTracks.Remove(audio);
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
}
