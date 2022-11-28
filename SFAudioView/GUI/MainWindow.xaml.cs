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
    private AudioProject? _project;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
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
    private void openMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var open = new OpenFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (open.ShowDialog() != true)
            return;

        _project?.Dispose();
        _project = new AudioProject(new[] { Audio.LoadFromFile(open.FileName) });
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
}
