using SFAudioCore.DataTypes;
using System.Windows;
using System.Windows.Controls;
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


    public void UpdateWaveform(float[] renderedData, uint sampleRate, uint channels)
    {
        // Add top points

        double width = ActualWidth;
        double height = ActualHeight;

        int samples = (int)(renderedData.Length / channels);

        for (int i = 0; i < renderedData.Length; i += (int)channels)
        {
        }

        // Add bottom points
    }

}
