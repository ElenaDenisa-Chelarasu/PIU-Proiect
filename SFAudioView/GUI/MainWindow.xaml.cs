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
using NAudio.Wave;

namespace SFAudio;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void HelloWorldButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Hello, world!");
    }

    private DirectSoundOut? output = null;
    private WaveFileReader? wave = null;

    private void DisposeWave()
    {
        if (output !=null)
        {
            if (output.PlaybackState == PlaybackState.Playing)
            {
                output.Stop();
            }
            output.Dispose();
            output = null;
        }
        if (wave != null)
        {
            wave.Dispose();
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
        OpenFileDialog open = new OpenFileDialog();
        open.Filter = "Wave File (*.wav)|*.wav;";
        if(open.ShowDialog() != true)
        {
            return;
        }
        
        DisposeWave();

        wave = new WaveFileReader(open.FileName);
        output = new DirectSoundOut();
        output.Init(new WaveChannel32(wave));
        output.Play();

        mainBarGui.pauseButton.IsEnabled = true;
    }
}
