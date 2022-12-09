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

public class AudioTrackVM : INotifyPropertyChanged
{
    private string _trackName = "";
    public string TrackName
    {
        get => _trackName;
        set
        {
            _trackName = value;
            NotifyChanged(nameof(TrackName));
        }
    }

    private void NotifyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Interaction logic for AudioTrack.xaml
/// </summary>
public partial class AudioTrack : UserControl
{
    public AudioTrackVM ViewModel { get; set; }

    public AudioTrack()
    {
        InitializeComponent();
        ViewModel = (AudioTrackVM)DataContext;
    }
}
