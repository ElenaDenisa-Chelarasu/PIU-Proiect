using SFAudioCore.DataTypes;
using System;
using System.Collections.Generic;
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
    public WaveformLogic()
    {
        InitializeComponent();
    }

    public void UpdateWaveform(float[] renderedData, uint sampleRate, uint channels, uint channel, uint sampleStart, uint sampleEnd)
    {
        int samples = (int)((sampleEnd - sampleStart) / channels);

        int samplesPerPoint = (int)(samples / Width);

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

        // Add points to polygon

        WaveformPolygon.Points.Clear();

        double xstep = Width / (samples / samplesPerPoint);

        double xpos = 0;

        foreach (var strength in topPoints)
        {
            WaveformPolygon.Points.Add(new Point(xpos, Height * (1.0 - strength) / 2.0));
            xpos += xstep;
        }

        foreach (var strength in bottomPoints.AsEnumerable().Reverse())
        {
            xpos -= xstep;
            WaveformPolygon.Points.Add(new Point(xpos, Height * (1.0 - strength) / 2.0));
        }
    }
}
