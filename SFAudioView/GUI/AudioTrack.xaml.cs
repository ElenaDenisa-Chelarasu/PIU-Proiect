﻿using SFAudioCore.DataTypes;
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

/// <summary>
/// Interaction logic for AudioTrack.xaml
/// </summary>
public partial class AudioTrack : UserControl
{
    public static readonly DependencyProperty AudioProperty 
        = DependencyProperty.Register(nameof(Audio), typeof(AudioInstance), typeof(AudioTrack), new UIPropertyMetadata());

    public static readonly DependencyProperty PlayPositionProperty
        = DependencyProperty.Register(nameof(PlayPosition), typeof(TimeSpan), typeof(AudioTrack), new UIPropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty RenderStartProperty
        = DependencyProperty.Register(nameof(RenderStart), typeof(TimeSpan), typeof(AudioTrack), new UIPropertyMetadata(TimeSpan.Zero));

    public static readonly DependencyProperty RenderDurationProperty
        = DependencyProperty.Register(nameof(RenderDuration), typeof(TimeSpan), typeof(AudioTrack), new UIPropertyMetadata(TimeSpan.FromSeconds(60)));

    public AudioInstance Audio
    {
        get => (AudioInstance)GetValue(AudioProperty);
        set => SetValue(AudioProperty, value);
    }

    public TimeSpan PlayPosition
    {
        get => (TimeSpan)GetValue(PlayPositionProperty);
        set => SetValue(PlayPositionProperty, value);
    }

    public TimeSpan RenderStart
    {
        get => (TimeSpan)GetValue(RenderStartProperty);
        set => SetValue(RenderStartProperty, value);
    }

    public TimeSpan RenderDuration
    {
        get => (TimeSpan)GetValue(RenderDurationProperty);
        set => SetValue(RenderDurationProperty, value);
    }

    public event EventHandler<WrappedValueEvent<TimeSpan>>? SelectionStarted;
    public event EventHandler<WrappedValueEvent<TimeSpan>>? SelectionUpdated;

    public AudioTrack()
    {
        InitializeComponent();
    }

    public event EventHandler? TrackRemoved;

    private void RemoveTrack_Click(object sender, RoutedEventArgs e)
    {
        TrackRemoved?.Invoke(this, EventArgs.Empty);
    }

    private void WaveformLeft_SelectionStarted(object sender, WrappedValueEvent<TimeSpan> e)
    {
        SelectionStarted?.Invoke(sender, e);
    }

    private void WaveformLeft_SelectionUpdated(object sender, WrappedValueEvent<TimeSpan> e)
    {
        SelectionUpdated?.Invoke(sender, e);
    }
}
