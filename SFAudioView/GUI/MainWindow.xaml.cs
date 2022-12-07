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

namespace SFAudio;

public class MainWindowVM : INotifyPropertyChanged
{
    public MainWindowVM()
    {
        void OnEngineTick(object? sender, EventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TimeCursorText)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DurationText)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LoopingText)));
            }
        }

        Engine.StateUpdated += OnEngineTick;
    }

    public AudioEngine Engine { get; } = new();

    private bool _shiftKeyHeld;
    public bool ShiftKeyHeld
    {
        get => _shiftKeyHeld;
        set
        {
            _shiftKeyHeld = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShiftKeyHeld)));
        }
    }

    private bool _altKeyHeld;
    public bool AltKeyHeld
    {
        get => _altKeyHeld;
        set
        {
            _altKeyHeld = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AltKeyHeld)));
        }
    }

    private bool _ctrlKeyHeld;
    public bool CtrlKeyHeld
    {
        get => _ctrlKeyHeld;
        set
        {
            _ctrlKeyHeld = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CtrlKeyHeld)));
        }
    }

    public string TimeCursorText => Engine.TimePosition.ToString("hh\\:mm\\:ss\\.fff");
    public string DurationText => Engine.Duration.ToString("hh\\:mm\\:ss\\.fff");
    public string LoopingText => Engine.Loop ? "Looping enabled." : "Looping disabled";

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowVM ViewModel { get; } = new();

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

        var first = new AudioInstance(Audio.LoadFromFile(open.FileName), TimeSpan.Zero);
        var second = new AudioInstance(Audio.LoadFromFile(Path.GetDirectoryName(open.FileName) + "/shadows.ogg"), TimeSpan.FromSeconds(5));

        first.Panning = -1;
        second.Panning = 1;

        ViewModel.Engine.SetAudio(new[]
        {
            first,
            second
        });
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

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            ViewModel.ShiftKeyHeld = true;

        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            ViewModel.AltKeyHeld = true;

        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            ViewModel.CtrlKeyHeld = true;

        Debug.WriteLine("Update2");
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            ViewModel.ShiftKeyHeld = false;

        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            ViewModel.AltKeyHeld = false;

        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            ViewModel.CtrlKeyHeld = false;

        Debug.WriteLine("Update");
    }

    private void Window_LostFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.ShiftKeyHeld = false;
        ViewModel.AltKeyHeld = false;
        ViewModel.CtrlKeyHeld = false;
    }

    private TimeSpan GetSkipTimeAmount()
    {
        TimeSpan baseTime = TimeSpan.FromSeconds(5);

        return (ViewModel.CtrlKeyHeld, ViewModel.ShiftKeyHeld, ViewModel.AltKeyHeld) switch
        {
            (true, _, _) => baseTime * 60,
            (_, true, _) => baseTime * 12,
            (_, _, true) => baseTime * 2,
            _ => baseTime
        };

    }
}
