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

public class WaveformLogicViewModel : ViewModelBase
{
    public AudioInstance? Audio { get; set; }

    private int _targetedChannel = 0;
    public int TargetedChannel
    {
        get => _targetedChannel;
        set => Change(ref _targetedChannel, value);
    }
}

/// <summary>
/// Interaction logic for WaveformLogic.xaml
/// </summary>
public partial class WaveformLogic : UserControl
{
    public WaveformLogicViewModel ViewModel => (WaveformLogicViewModel)DataContext;

    public WaveformLogic()
    {
        InitializeComponent();
    }

    private void UpdateWaveform()
    {
        WaveformPolygon.Points.Clear();

        if (ViewModel.Audio == null)
            return;

        int sampleStart = 0;
        int sampleEnd = ViewModel.Audio.Source.SampleCount;
        int channels = ViewModel.Audio.Source.Channels;
        int channel = ViewModel.TargetedChannel;
        float[] renderedData = ViewModel.Audio.Source.Data;

        int samples = sampleEnd - sampleStart;

        int samplesPerPoint = (int)(samples / ActualWidth + 1);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        int samplesAccumulated = 0;

        var topPoints = new List<float>();
        var bottomPoints = new List<float>();

        for (int i = (int)(channel + sampleStart * channels); i < sampleEnd * channels; i += (int)channels)
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
        UpdateWaveform();
    }
}
