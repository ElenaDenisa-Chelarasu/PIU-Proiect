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
using SFAudioView.ViewModels;

namespace SFAudioView.GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public event EventHandler<WrappedValueEvent<string>>? FileOpened;
    public event EventHandler<WrappedValueEvent<string>>? FileSaved;
    public event EventHandler<WrappedValueEvent<AudioInstance>>? TrackRemoved;

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

        FileOpened?.Invoke(this, new(open.FileName));
    }

    private void saveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var save = new SaveFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (save.ShowDialog() != true || save.FileName == "")
            return;

        FileSaved?.Invoke(this, new(save.FileName));
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
        var save = new SaveFileDialog
        {
            Filter = "Vorbis File (*.ogg)|*.ogg;|Wave File (*.wav)|*.wav;"
        };

        if (save.ShowDialog() != true || save.FileName == "")
            return;

        FileSaved?.Invoke(this, new(save.FileName));
    }

    private void OnTrackRemoved(object? sender, EventArgs e)
    {
        if (sender is AudioTrack audioTrack)
        {
            TrackRemoved?.Invoke(this, new(audioTrack.Audio));
        }
    }
}
