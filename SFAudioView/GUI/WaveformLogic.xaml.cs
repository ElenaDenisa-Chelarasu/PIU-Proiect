using SFAudioCore.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Shapes;


namespace SFAudioView.GUI;

/// <summary>
/// Interaction logic for WaveformLogic.xaml
/// </summary>
public partial class WaveformLogic : UserControl
{
    public static readonly DependencyProperty AudioProperty
        = DependencyProperty.Register(nameof(Audio), typeof(AudioInstance), typeof(WaveformLogic), new UIPropertyMetadata(new AudioInstance(Array.Empty<float>(), 2, 44100)));

    public static readonly DependencyProperty TargetedChannelProperty
        = DependencyProperty.Register(nameof(TargetedChannel), typeof(int), typeof(WaveformLogic), new UIPropertyMetadata(0, OnPropertyChanged));

    public static readonly DependencyProperty PlayPositionProperty
        = DependencyProperty.Register(nameof(PlayPosition), typeof(TimeSpan), typeof(WaveformLogic), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public AudioInstance Audio
    {
        get => (AudioInstance)GetValue(AudioProperty);
        set => SetValue(AudioProperty, value);
    }

    public int TargetedChannel
    {
        get => (int)GetValue(TargetedChannelProperty);
        set => SetValue(TargetedChannelProperty, value);
    }

    public TimeSpan PlayPosition
    {
        get => (TimeSpan)GetValue(PlayPositionProperty);
        set => SetValue(PlayPositionProperty, value);
    }

    public WaveformLogic()
    {
        InitializeComponent();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WaveformLogic waveform)
        {
            waveform.UpdatePlayPosition();

            if (e.Property != PlayPositionProperty)
                waveform.UpdateWaveform();
        }
    }

    private void UpdatePlayPosition()
    {
        if (Audio.Channels == 0 || ActualWidth == 0)
            return;

        int samplePosition = (int)(PlayPosition.TotalSeconds * Audio.SampleRate);

        int clampedPos = Math.Clamp(samplePosition, 0, Audio.Data.Length / Audio.Channels);

        double xpos = ActualWidth * clampedPos / Audio.Data.Length;

        PlayPositionPolygon.Points.Clear();
        PlayPositionPolygon.Points.Add(new(xpos, 0));
        PlayPositionPolygon.Points.Add(new(xpos, ActualHeight));
        PlayPositionPolygon.Points.Add(new(xpos + 1, ActualHeight));
        PlayPositionPolygon.Points.Add(new(xpos + 1, 0));
    }

    private void UpdateWaveform()
    {
        if (Audio.Channels == 0 || ActualWidth == 0)
            return;

        WaveformPolygon.Points.Clear();

        int sampleStart = 0;
        int sampleEnd = Audio.Data.Length / Audio.Channels;
        ReadOnlySpan<float> renderedData = Audio.Data.Span;

        int samples = sampleEnd - sampleStart;

        int samplesPerPoint = (int)(samples / ActualWidth + 1);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        int samplesAccumulated = 0;

        var topPoints = new List<float>();
        var bottomPoints = new List<float>();

        for (int i = sampleStart * Audio.Channels + TargetedChannel; i < sampleEnd * Audio.Channels; i += Audio.Channels)
        {
            minValue = Math.Min(minValue, renderedData[i]);
            maxValue = Math.Max(maxValue, renderedData[i]);

            samplesAccumulated++;

            if (samplesAccumulated >= samplesPerPoint)
            {
                samplesAccumulated -= samplesPerPoint;
                topPoints.Add(maxValue);
                bottomPoints.Add(minValue);

                minValue = float.MaxValue;
                maxValue = float.MinValue;
            }
        }

        double xstep = ActualWidth / (samples / samplesPerPoint);

        double xpos = 0;

        foreach (var strength in topPoints)
        {
            WaveformPolygon.Points.Add(new Point(xpos, ActualHeight * (1.0 - strength) / 2.0));
            xpos += xstep;
        }

        foreach (var strength in bottomPoints.AsEnumerable().Reverse())
        {
            xpos -= xstep;
            WaveformPolygon.Points.Add(new Point(xpos, ActualHeight * (1.0 - strength) / 2.0));
        }
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePlayPosition();
        UpdateWaveform();
    }
}
