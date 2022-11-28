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
    private Audio? _audio;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void HelloWorldButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Hello, world!");
    }

    private SoundBuffer? _buffer;
    private Sound? _sound;

    private void DisposeWave()
    {
        if (_sound != null)
        {
            if (_sound.Status == SoundStatus.Playing)
            {
                _sound.Stop();
            }

            _sound.Dispose();
            _sound = null;
        }

        if (_buffer != null)
        {
            _buffer.Dispose();
            waveMenuItem = null;
        }
    }

    /// <summary>
    /// Deschide un fisier de tip wav si ruleaza impicit
    /// TO DO: Sa nu se randeze implicit, ci doar sa deschida si sa salveze acest state, 
    /// pentru a pute fi folosit in randarea unei imagini Waveform
    /// </summary>
    private void openMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var open = new OpenFileDialog
        {
            Filter = "Wave File (*.wav)|*.wav;|Vorbis File (*.ogg)|*.ogg;"
        };

        if (open.ShowDialog() != true)
            return;

        DisposeWave();

        _audio = Audio.LoadFromFile(open.FileName);

        mainBarGui.pauseButton.IsEnabled = true;
    }

    public void playButton_Click(object sender, RoutedEventArgs e)
    {
        if (_audio == null)
            return;

        _buffer ??= _audio.MakeBuffer();
        _sound ??= new Sound(_buffer);
        
        _sound.Play();
    }

    public void stopButton_Click(object sender, RoutedEventArgs e)
    {
        _sound?.Stop();
    }
}
