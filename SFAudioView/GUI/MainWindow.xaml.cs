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

namespace SFAudio;

public class MainWindowVM : INotifyPropertyChanged
{
    public MainWindowVM()
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
        set
        {
            _actionDescriptionText = value;
            NotifyChanged(nameof(ActionDescriptionText));
        }
    }

    public string SampleRateText => Engine.SampleRate + " Hz";
    public string TimeCursorText => Engine.TimePosition.ToString("hh\\:mm\\:ss\\.fff");
    public string DurationText => Engine.Duration.ToString("hh\\:mm\\:ss\\.fff");
    public string LoopingText => Engine.Loop ? "Looping On." : "Looping Off.";

    private void NotifyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowVM ViewModel { get; } = new();

    public List<AudioTrack> AudioTracks { get; } = new(); 

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = (MainWindowVM)DataContext;
    }

    private void HelloWorldButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Hello, world!");
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

        ViewModel.Engine.SetAudio(new[]
        {
            audio
        });

        AudioTracks.Clear();
        AudioTracks.Add(new AudioTrack()
        {
            ViewModel =
            {
                TrackName = Path.GetFileName(open.FileName)
            }
        });

        var track = AudioTracks.Last();

        if (audio.Source.Channels == 2)
        {
            var waves = new List<WaveformLogic>()
            {
                new WaveformLogic() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Height = 25 },
                new WaveformLogic() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Height = 25 }
            };

            for (int i = 0; i < audio.Source.Data.Length; i += 2)
            {
                waves[0].AddValue(audio.Source.Data[i], -audio.Source.Data[i]);
                waves[1].AddValue(audio.Source.Data[i + 1], -audio.Source.Data[i + 1]);
            }

            track.WaveformItems.ItemsSource = waves;
        }

        else
        {
            var waves = new List<WaveformLogic>()
            {
                new WaveformLogic() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) }
            };

            for (int i = 0; i < audio.Source.Data.Length; i++)
            {
                waves[0].AddValue(audio.Source.Data[i], audio.Source.Data[i]);
            }

            track.WaveformItems.ItemsSource = waves;
        }

        AudioTrackItems.ItemsSource = AudioTracks;
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

    private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        const string copyright =
                "Audio Pley and Mixer\r\n" +
                "Proiectarea Interfetelor Utilizator, Proiect\r\n" +
                "(c)2022 Chelarasu Elena-Denisa, Miron Alexandru\r\n";

        MessageBox.Show(copyright, "Despre proiect");
    }
}
