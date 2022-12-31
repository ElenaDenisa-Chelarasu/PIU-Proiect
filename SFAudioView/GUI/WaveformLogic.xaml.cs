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

    public static readonly DependencyProperty RenderStartProperty
        = DependencyProperty.Register(nameof(RenderStart), typeof(TimeSpan), typeof(WaveformLogic), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public static readonly DependencyProperty RenderDurationProperty
        = DependencyProperty.Register(nameof(RenderDuration), typeof(TimeSpan), typeof(WaveformLogic), new UIPropertyMetadata(TimeSpan.FromSeconds(60), OnPropertyChanged));

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
        if (Audio.Channels == 0 || MainCanvas.ActualWidth == 0)
            return;

        TimeSpan clampedPosition = TimeSpan.FromSeconds(Math.Clamp(PlayPosition.TotalSeconds - RenderStart.TotalSeconds, 0, RenderDuration.TotalSeconds));

        double xpos = MainCanvas.ActualWidth * clampedPosition / RenderDuration;

        PlayPositionPolygon.Points.Clear();
        PlayPositionPolygon.Points.Add(new(xpos, 0));
        PlayPositionPolygon.Points.Add(new(xpos, MainCanvas.ActualHeight));
        PlayPositionPolygon.Points.Add(new(xpos + 1, MainCanvas.ActualHeight));
        PlayPositionPolygon.Points.Add(new(xpos + 1, 0));
    }

    private void UpdateWaveform()
    {
        // CACHE THE GODDAMN DEPENDENCY PROPERTY VALUE OR THE VALUES YOU GET FROM IT
        // OTHERWISE THE PERFORMANCE GOES TO SHIT BECAUSE YOU KEEP CALLING FRAMEWORK METHODS IN THE HOT PATH LOOP
        AudioInstance cachedAudio = Audio;
        double actualHeight = MainCanvas.ActualHeight;
        double actualWidth = MainCanvas.ActualWidth;

        if (cachedAudio.Channels == 0 || MainCanvas.ActualWidth == 0)
            return;

        // Not all of the sample data is available in the given portion
        // The Render region is the time that should be rendered, the actual sample available may be a fraction of that region


        int sampleStartDesired = (int)(RenderStart.TotalSeconds * cachedAudio.SampleRate);
        int samplesDesired = (int)(RenderDuration.TotalSeconds * cachedAudio.SampleRate);
        int sampleEndDesired = sampleStartDesired + samplesDesired;

        int sampleStart = Math.Clamp(sampleStartDesired, 0, cachedAudio.SampleCount);
        int sampleEnd = Math.Clamp(sampleEndDesired, 0, cachedAudio.SampleCount);
        int samples = sampleEnd - sampleStart;

        ReadOnlySpan<float> renderedData = cachedAudio.Data.Span;

        int samplesPerPoint = (int)(samplesDesired / actualWidth);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        int samplesAccumulated = 0;

        int pointCount = samples / samplesPerPoint;

        var topPoints = new float[pointCount];
        var bottomPoints = new float[pointCount];

        // Safe version of this code is too slow
        // Therefore unsafe code was used for sample rendering, yay!
        unsafe
        {
            unchecked
            {
                int index = 0;
                int renderedPointIndex = 0;
                int count = samples * cachedAudio.Channels;
                
                fixed (float* data = &renderedData[sampleStart * cachedAudio.Channels + TargetedChannel])
                {
                    while (index < count)
                    {
                        minValue = Math.Min(minValue, *(data + index));
                        maxValue = Math.Max(maxValue, *(data + index));
                        index += cachedAudio.Channels;

                        samplesAccumulated++;

                        if (samplesAccumulated >= samplesPerPoint)
                        {
                            samplesAccumulated -= samplesPerPoint;
                            topPoints[renderedPointIndex] = maxValue;
                            bottomPoints[renderedPointIndex] = minValue;
                            renderedPointIndex++;

                            minValue = float.MaxValue;
                            maxValue = float.MinValue;
                        }
                    }
                }
            }
        }

        double xstep = actualWidth / (samplesDesired / (float)samplesPerPoint);

        double xpos = (sampleStartDesired - sampleStart) * xstep;

        var points = new PointCollection(pointCount);

        unsafe
        {
            int index = 0;

            fixed (float* strength = topPoints)
            {
                while (index < pointCount)
                {
                    points.Add(new Point(xpos, actualHeight * (1.0 - *(strength + index)) / 2.0));
                    xpos += xstep;
                    index++;
                }
            }

            index = pointCount - 1;

            fixed (float* strength = bottomPoints)
            {
                while (index > 0)
                {
                    xpos -= xstep;
                    points.Add(new Point(xpos, actualHeight * (1.0 - *(strength + index)) / 2.0));
                    index--;
                }
            }
        }

        WaveformPolygon.Points = points;
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePlayPosition();
        UpdateWaveform();
    }
}
