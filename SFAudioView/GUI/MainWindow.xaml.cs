using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using SFML.Audio;
using SFAudioCore.DataTypes;
using System.ComponentModel;
using Path = System.IO.Path;
using System.Diagnostics;
using SFML.System;
using SFAudioView.GUI;
using System.Windows.Markup;

namespace SFAudio;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        void OnEngineTick(object? sender, EventArgs e)
        {
            NotifyChanged(nameof(SampleRateText));
            NotifyChanged(nameof(TimeCursorText));
            NotifyChanged(nameof(DurationText));
            NotifyChanged(nameof(LoopingText));
        }

        Engine.StateUpdated += OnEngineTick;
    }

    public AudioEngine Engine { get; } = new();

    private string _actionDescriptionText = "";
    public string ActionDescriptionText
    {
        get => _actionDescriptionText;
        set => Change(ref _actionDescriptionText, value);
    }

    private TimeSpan _playRegionSize;
    public TimeSpan PlayRegionSize
    {
        get => _playRegionSize;
        set => Change(ref _playRegionSize, value);
    }

    private TimeSpan _playRegionStart;
    public TimeSpan PlayRegionStart
    {
        get => _playRegionStart;
        set => Change(ref _playRegionStart, value);
    }

    public string SampleRateText => Engine.SampleRate + " Hz";
    public string TimeCursorText => Engine.TimePosition.ToString("hh\\:mm\\:ss\\.fff");
    public string DurationText => Engine.Duration.ToString("hh\\:mm\\:ss\\.fff");
    public string LoopingText => Engine.Loop ? "Looping On." : "Looping Off.";
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Deschide un fisier de tip wav si ruleaza impicit
    /// TO DO: Sa nu se randeze implicit, ci doar sa deschida si sa salveze acest state, 
    /// pentru a pute fi folosit in randarea unei imagini Waveform
    /// </summary>
    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var open = new OpenFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (open.ShowDialog() != true)
            return;

        ViewModel.Engine.Stop();

        var audio = new AudioInstance(Audio.LoadFromFile(open.FileName), TimeSpan.Zero);
        //var audio = new AudioInstance(Audio.WhiteNoise(2, 44100, TimeSpan.FromSeconds(20)), TimeSpan.Zero);


        ViewModel.Engine.AddAudio(audio);

        AudioTrack audioTrack;

        AudioTrackItems.Items.Add(audioTrack = new AudioTrack()
        {
            ViewModel =
            {
                Audio = audio,
                TrackName = Path.GetFileName(open.FileName)
            },
            WaveformLeft = { ViewModel = { Audio = audio } },
            WaveformRight = { ViewModel = { Audio = audio } }
        });

        audioTrack.TrackRemoved += OnTrackRemoved;

        //float[] data = ViewModel.Engine.Render(TimeSpan.Zero, ViewModel.Engine.Duration);
    }

    public void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.Play();
    }

    public void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.Pause();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.Stop();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }

    private void SkipLeftButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.TimePosition -= GetSkipTimeAmount();
    }

    private void SkipRightButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.TimePosition += GetSkipTimeAmount();
    }

    private void LoopButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Engine.Loop = !ViewModel.Engine.Loop;
    }

    private TimeSpan GetSkipTimeAmount()
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

    private void Window_OnKeyDown(object sender, KeyEventArgs e)
    {
        ProcessDescriptionUpdate();
    }

    private void Window_OnKeyUp(object sender, KeyEventArgs e)
    {
        ProcessDescriptionUpdate();
    }

    private void GenericDescription_MouseOver(object sender, MouseEventArgs e)
    {
        ProcessDescriptionUpdate();
    }

    private void ProcessDescriptionUpdate()
    {
        Dictionary<Control, string> templates = new()
        {
            [PlayButton] = "Start or resume audio playback.",
            [PauseButton] = "Pause audio playback.",
            [StopButton] = "Stop audio playback and go back to start.",
            [SkipLeftButton] = "({Modifier}) Skip {TimeSkip} backward.",
            [SkipRightButton] = "({Modifier}) Skip {TimeSkip} forward.",
            [LoopButton] = "Enable or disable looping."
        };

        var mouseOver = templates.Keys.FirstOrDefault(x => x.IsMouseOver);

        if (mouseOver == null || !templates.TryGetValue(mouseOver, out string? text))
            text = "";

        else
        {
            string modifier = " - ";

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                modifier = "Ctrl";

            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                modifier = "Shift";

            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                modifier = "Alt";

            text = text.Replace("{Modifier}", modifier);
            text = text.Replace("{TimeSkip}", GetSkipTimeAmount().ToString());
        }

        ViewModel.ActionDescriptionText = text;
    }

    private void saveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var temp_samples = ViewModel.Engine.Render(TimeSpan.Zero, ViewModel.Engine.Duration);
        var samples = new short[temp_samples.Length];
        AudioConvert.ConvertFloatTo16(temp_samples, samples);
        var buffer = new SoundBuffer(samples, 2, 44100);

        var save = new SaveFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (save.ShowDialog() != true)
            return;
        if (save.FileName != "")
        {
            buffer.SaveToFile(save.FileName);
        }
    }

    private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        const string copyright =
                "Audio Pley and Mixer\r\n" +
                "Proiectarea Interfetelor Utilizator, Proiect\r\n" +
                "(c)2022 Chelarasu Elena-Denisa, Miron Alexandru\r\n";

        MessageBox.Show(copyright, "Despre proiect");
    }

    private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var temp_samples = ViewModel.Engine.Render(TimeSpan.Zero, ViewModel.Engine.Duration);
        var samples = new short[temp_samples.Length];
        AudioConvert.ConvertFloatTo16(temp_samples, samples);
        var buffer = new SoundBuffer(samples, 2, 44100);

        var save = new SaveFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (save.ShowDialog() != true)
            return;
        if (save.FileName != "")
        {
            buffer.SaveToFile(save.FileName);
        }
    }

    private void OnTrackRemoved(object? sender, EventArgs e)
    {
        if (sender is AudioTrack audioTrack)
        {
            var audio = audioTrack.ViewModel.Audio;
            
            if (audio != null)
                ViewModel.Engine.RemoveAudio(audio);

            AudioTrackItems.Items.Remove(audioTrack);
        }
    }
}
