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

public record SelectionState(
    TimeSpan FirstPoint,
    TimeSpan LastPoint,
    AudioInstance? Audio,
    int? Channel);

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
    public DelegateCommand<WrappedValueEvent<SelectionUpdate?>> SelectionUpdateCommand { get; }
    public DelegateCommand<WrappedValueEvent<double>> AmplifySelectionCommand { get; }

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
            Notify(nameof(TotalDuration));
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
        SelectionUpdateCommand = new((e) => UpdateSelection(e.Value));
        AmplifySelectionCommand = new((e) => AmplifySelection(e.Value));
    }

    public ObservableCollection<AudioInstance> AudioTracks { get; } = new();

    private string _actionDescriptionText = "";
    public string ActionDescriptionText
    {
        get => _actionDescriptionText;
        set { _actionDescriptionText = value; Notify(); }
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
            _scrollZoom = Math.Clamp(value, 1f, 8f); 
            Notify();
            Notify(nameof(RenderStart));
            Notify(nameof(RenderDuration));
        }
    }
    private double _scrollZoom = 5;

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
            double ratio = _scrollZoom - Math.Floor(_scrollZoom);

            TimeSpan T1 = TimeSpan.FromMilliseconds(50);
            TimeSpan T2 = TimeSpan.FromMilliseconds(250);
            TimeSpan T3 = TimeSpan.FromSeconds(1);
            TimeSpan T4 = TimeSpan.FromSeconds(5);
            TimeSpan T5 = TimeSpan.FromSeconds(25);
            TimeSpan T6 = TimeSpan.FromMinutes(1);
            TimeSpan T7 = TimeSpan.FromMinutes(5);
            TimeSpan T8 = TimeSpan.FromMinutes(25);

            TimeSpan baseValue = _scrollZoom switch
            {
                < 1 => T1,
                >= 1 and < 2 => Lerp(T1, T2, ratio),
                >= 2 and < 3 => Lerp(T2, T3, ratio),
                >= 3 and < 4 => Lerp(T3, T4, ratio),
                >= 4 and < 5 => Lerp(T4, T5, ratio),
                >= 5 and < 6 => Lerp(T5, T6, ratio),
                >= 6 and < 7 => Lerp(T6, T7, ratio),
                >= 7 and < 8 => Lerp(T7, T8, ratio),
                >= 8 => T8,
                _ => T8
            };

            return TimeSpan.FromSeconds(Math.Clamp(baseValue.TotalSeconds, 0, Engine.Duration.TotalSeconds));
        }
    }

    private static TimeSpan Lerp(TimeSpan a, TimeSpan b, double ratio)
    {
        return a * (1 - ratio) + b * ratio;
    }

    public SelectionState? SelectionState
    {
        get => _selectionState;
        set { _selectionState = value; Notify(); }
    }
    private SelectionState? _selectionState;

    private void UpdateSelection(SelectionUpdate? update)
    {
        if (update == null)
            SelectionState = null;

        else if (update.IsFirstPoint)
        {
            SelectionState = new(
                update.Point,
                update.Point,
                update.Audio,
                update.Channel);
        }
        else if (SelectionState is not null)
        {
            SelectionState = new(
                SelectionState.FirstPoint,
                update.Point,
                SelectionState.Audio,
                SelectionState.Channel);
        }
    }

    private void AmplifySelection(double ratio)
    {
        var state = SelectionState;

        if (state is null || state.Audio is null)
            return;

        var audio = state.Audio;

        var (start, end) = (state.FirstPoint, state.LastPoint);

        if (state.FirstPoint > state.LastPoint)
            (start, end) = (end, start);

        int sampleStart = (int)(start.TotalSeconds * audio.SampleRate);
        int sampleEnd = (int)(end.TotalSeconds * audio.SampleRate);

        AudioEffects.Amplify(state.Audio, sampleStart, sampleEnd, (float)ratio, state.Channel);

        SelectionState = null;
    }
}
