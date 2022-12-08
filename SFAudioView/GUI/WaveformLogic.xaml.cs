using SFAudioCore.DataTypes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace SFAudioView.GUI
{


    /// <summary>
    /// Interaction logic for WaveformLogic.xaml
    /// </summary>
    public partial class WaveformLogic : UserControl
    {
        public DependencyProperty SampleAggregatorProperty = DependencyProperty.Register("SampleAggregator", 
                                                                                        typeof(SampleAggregator),
                                                                                        typeof(WaveformLogic),
                                                                                        new PropertyMetadata(null, OnSampleAggregatorChanged));

        public SampleAggregator SampleAggregator
        {
            get { return (SampleAggregator)GetValue(SampleAggregatorProperty); }
            set { SetValue(SampleAggregatorProperty, value); }
        }

        private static void OnSampleAggregatorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (WaveformLogic)sender;
            control.Subscribe();
        }

        public void Subscribe()
        {
            System.EventHandler<MaximumDataSetEventArgs> onMaximumCalculated = OnMaximumCalculated;
            SampleAggregator.MaximumCalculated += onMaximumCalculated;
        }

        void OnMaximumCalculated(object? sender, MaximumDataSetEventArgs e)
        {
            if (IsEnabled)
            {
                this.AddValue(e.MaxSample, e.MinSample);
            }
        }


        private int renderPosition;
        private double xScale = 2;
        private double yScale = 40;
        private int blankZone = 10;
        private double yTranslate = 40;

        Polygon waveForm = new Polygon();

        /// <summary>
        /// Setting the visuals of the WaveForm and adding it to the mainCanvas
        /// </summary>
        public WaveformLogic()
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();
            waveForm.Stroke = Foreground;
            waveForm.StrokeThickness = 1;
            waveForm.Fill = new SolidColorBrush(Colors.Aquamarine);
            mainCanvas.Children.Add(waveForm);
        }

        /// <summary>
        /// Getter/Setter for the multitude of Points of the WaveForm
        /// </summary>
        private int Points
        {
            get { return waveForm.Points.Count / 2; }
        }

        /// <summary>
        /// Calculates the position of the last/lowest point
        /// </summary>
        private int BottomPointIndex(int position)
        {
            return waveForm.Points.Count - position - 1;
        }

        /// <summary>
        /// Calculates the position of the current point
        /// </summary>
        private double SampleToYPosition(float value)
        {
            return yTranslate + value * yScale;
        }

        /// <summary>
        /// Inserts a new point in the Wave FOrm
        /// If we have the first point, we add its 2 coordinates at the end
        /// Else calculate the position in the points array
        /// </summary>
        private void CreatePoint(float topValue, float bottomValue)
        {
            var topYPos = SampleToYPosition(topValue);
            var bottomYPos = SampleToYPosition(bottomValue);
            var xPos = renderPosition * xScale;
            if (renderPosition >= Points)
            {
                int insertPos = Points;
                waveForm.Points.Insert(insertPos, new Point(xPos, topYPos));
                waveForm.Points.Insert(insertPos + 1, new Point(xPos, bottomYPos));
            }
            else
            {
                waveForm.Points[renderPosition] = new Point(xPos, topYPos);
                waveForm.Points[BottomPointIndex(renderPosition)] = new Point(xPos, bottomYPos);
            }
            renderPosition++;
        }

        /// <summary>
        /// When we get a new sample maximum and minimum in, we have to insert
        /// those values into the middle of the existing Points collection, or calculate the position in the points array
        /// </summary>
        public void AddValue(float maxValue, float minValue)
        {
            var visiblePixels = (int)(ActualWidth / xScale);
            if (visiblePixels > 0)
            {
                CreatePoint(maxValue, minValue);

                if (renderPosition > visiblePixels)
                {
                    renderPosition = 0;
                }
                int erasePosition = (renderPosition + blankZone) % visiblePixels;
                if (erasePosition < Points)
                {
                    double yPos = SampleToYPosition(0);
                    waveForm.Points[erasePosition] = new Point(erasePosition * xScale, yPos);
                    waveForm.Points[BottomPointIndex(erasePosition)] = new Point(erasePosition * xScale, yPos);
                }
            }
        }

        /// <summary>
        /// Resetting an rescalling vertically
        /// </summary>
        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We will remove everything as we are going to rescale vertically
            Reset();

            yTranslate = ActualHeight / 2;
            yScale = ActualHeight / 2;
        }

        /// <summary>
        /// Clears all points
        /// </summary>
        private void ClearAllPoints()
        {
            waveForm.Points.Clear();
        }

        /// <summary>
        /// Clears the waveform and repositions on the left
        /// </summary>
        public void Reset()
        {
            renderPosition = 0;
            ClearAllPoints();
        }
    }
}
