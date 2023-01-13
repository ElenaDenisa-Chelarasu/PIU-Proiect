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

public record SelectionUpdate(
    TimeSpan Point,
    bool IsFirstPoint,
    AudioInstance Audio,
    int? Channel);

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : GlobalAudioSelectionWindowBase
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public event EventHandler<WrappedValueEvent<string>>? FileOpened;
    public event EventHandler<WrappedValueEvent<string>>? FileSaved;
    public event EventHandler<WrappedValueEvent<AudioInstance>>? TrackRemoved;
    public event EventHandler<WrappedValueEvent<SelectionUpdate?>>? SelectionUpdated;
    public event EventHandler<WrappedValueEvent<double>>? EffectAmplify;

    /// <summary>
    /// Deschide un fisier de tip wav si ruleaza impicit
    /// TO DO: Sa nu se randeze implicit, ci doar sa deschida si sa salveze acest state, 
    /// pentru a pute fi folosit in randarea unei imagini Waveform
    /// </summary>
    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var open = new OpenFileDialog
        {
            Filter =
                "Vorbis File (*.ogg)|*.ogg;" +
                "|Wave File (*.wav)|*.wav;" +
                "|MP3 File (*.mp3)|*.mp3;"
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

    private void AudioTrack_SelectionStarted(object sender, WrappedValueEvent<TimeSpan> e)
    {
        if (sender is not WaveformLogic waveform)
            return;

        var arg = new SelectionUpdate(
            e.Value,
            true,
            waveform.Audio,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? null : waveform.TargetedChannel
            );

        SelectionUpdated?.Invoke(this, new WrappedValueEvent<SelectionUpdate?>(arg));
    }

    private void AudioTrack_SelectionUpdated(object sender, WrappedValueEvent<TimeSpan> e)
    {
        if (sender is not WaveformLogic waveform)
            return;

        var arg = new SelectionUpdate(
            e.Value,
            false,
            waveform.Audio,
            Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? null : waveform.TargetedChannel
            );

        SelectionUpdated?.Invoke(this, new WrappedValueEvent<SelectionUpdate?>(arg));
    }

    private void GlobalAudioSelectionWindowBase_SelectionStateUpdated(object sender, WrappedValueEvent<SelectionState?> e)
    {
        var state = e.Value;

        foreach (var item in AudioTrackItems.Items)
        {
            var control = AudioTrackItems.ItemContainerGenerator.ContainerFromItem(item);
            var track = FindVisualChild<AudioTrack>(control);

            if (track != null)
            {
                if (state == null || track.Audio != state.Audio)
                {
                    track.WaveformLeft.SelectionFirstPoint = TimeSpan.Zero;
                    track.WaveformLeft.SelectionLastPoint = TimeSpan.Zero;
                    track.WaveformRight.SelectionFirstPoint = TimeSpan.Zero;
                    track.WaveformRight.SelectionLastPoint = TimeSpan.Zero;
                }
                else
                {
                    var both = state.Channel == null;

                    if (both || state.Channel == track.WaveformLeft.TargetedChannel)
                    {
                        track.WaveformLeft.SelectionFirstPoint = state.FirstPoint;
                        track.WaveformLeft.SelectionLastPoint = state.LastPoint;
                    }
                    else
                    {
                        track.WaveformLeft.SelectionFirstPoint = TimeSpan.Zero;
                        track.WaveformLeft.SelectionLastPoint = TimeSpan.Zero;
                    }

                    if (both || state.Channel == track.WaveformRight.TargetedChannel)
                    {
                        track.WaveformRight.SelectionFirstPoint = state.FirstPoint;
                        track.WaveformRight.SelectionLastPoint = state.LastPoint;
                    }
                    else
                    {
                        track.WaveformRight.SelectionFirstPoint = TimeSpan.Zero;
                        track.WaveformRight.SelectionLastPoint = TimeSpan.Zero;
                    }
                }
            }
        }
    }

    public static T? FindVisualChild<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }

                T? childItem = FindVisualChild<T>(child);
                if (childItem != null) return childItem;
            }
        }
        return null;
    }

    private void Amplify_Click(object sender, RoutedEventArgs e)
    {
        if (SelectionState == null)
        {
            MessageBox.Show("Select some audio first!");
            return;
        }

        var dialog = new AmplifyEffectDialog();

        if (!dialog.ShowDialog().GetValueOrDefault(false))
            return;

        EffectAmplify?.Invoke(this, new WrappedValueEvent<double>(dialog.Result));
    }
}
