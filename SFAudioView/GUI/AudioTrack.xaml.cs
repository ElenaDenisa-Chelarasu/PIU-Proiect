using SFAudioCore.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SFAudioView.GUI;

public class AudioTrackViewModel : ViewModelBase
{
    public AudioInstance? Audio { get; set; }

    private string _trackName = "";
    public string TrackName
    {
        get => _trackName;
        set => Change(ref _trackName, value);
    }
}

/// <summary>
/// Interaction logic for AudioTrack.xaml
/// </summary>
public partial class AudioTrack : UserControl
{
    public AudioTrackViewModel ViewModel => (AudioTrackViewModel)DataContext;

    public event EventHandler? TrackRemoved;

    public AudioTrack()
    {
        InitializeComponent();
    }

    private void VolumeLeft_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.VolumeLeft = (float)e.NewValue;
    }

    private void VolumeRight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.VolumeRight = (float)e.NewValue;
    }

    private void MuteLeft_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.MuteLeft = true;
    }

    private void MuteLeft_Unchecked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.MuteLeft = false;
    }

    private void MuteRight_Checked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.MuteRight = true;
    }

    private void MuteRight_Unchecked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Audio == null)
            return;

        ViewModel.Audio.MuteRight = false;
    }

    private void RemoveTrack_Click(object sender, RoutedEventArgs e)
    {
        TrackRemoved?.Invoke(this, e);
    }
}
