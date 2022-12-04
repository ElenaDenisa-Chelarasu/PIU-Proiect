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

namespace SFAudio;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private AudioEngine _project = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _project.PlaybackChanged += OnPlaybackChanged;

        OnPlaybackChanged(this, new AudioEngine.PlaybackChangedArgs()
        {
            Looping = false,
            Playing = false,
            TimeCursor = TimeSpan.Zero
        });
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

        _project.Playing = false;
        _project.TimeCursor = TimeSpan.Zero;

        _project.UpdateSamples(new[]
        {
            new AudioSample(Audio.LoadFromFile(open.FileName), TimeSpan.Zero),
            new AudioSample(Audio.LoadFromFile(open.FileName), TimeSpan.FromSeconds(10))
        });
    }

    public void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_project != null)
        {
            _project.Playing = true;
        }
    }

    public void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_project != null)
        {
            _project.Playing = false;
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        if (_project != null)
        {
            _project.Playing = false;
            _project.TimeCursor = TimeSpan.Zero;
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _project?.Dispose();
    }

    private void OnPlaybackChanged(object? sender, AudioEngine.PlaybackChangedArgs e)
    {
        /*
        if (_project.SampleCount != 0)
        {
            if (_project.Playing)
            {
                StopButton.IsEnabled = true;
                PauseButton.IsEnabled = true;
                PlayButton.IsEnabled = false;
            }
            else
            {
                StopButton.IsEnabled = false;
                PauseButton.IsEnabled = false;
                PlayButton.IsEnabled = true;
            }
        }
        else
        {
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            PlayButton.IsEnabled = false;
        }
        */
    }
}
