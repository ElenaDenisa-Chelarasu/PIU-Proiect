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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SFAudioView.GUI;

/// <summary>
/// Interaction logic for PlayIndicator.xaml
/// </summary>
public partial class PlayIndicator : UserControl
{
    public static readonly DependencyProperty PlayPositionProperty
        = DependencyProperty.Register(nameof(PlayPosition), typeof(TimeSpan), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public static readonly DependencyProperty RenderStartProperty
        = DependencyProperty.Register(nameof(RenderStart), typeof(TimeSpan), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public static readonly DependencyProperty RenderDurationProperty
        = DependencyProperty.Register(nameof(RenderDuration), typeof(TimeSpan), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public static readonly DependencyProperty TotalDurationProperty
        = DependencyProperty.Register(nameof(TotalDuration), typeof(TimeSpan), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.Zero, OnPropertyChanged));

    public static readonly DependencyProperty SelectionFirstPointProperty
        = DependencyProperty.Register(nameof(SelectionFirstPoint), typeof(TimeSpan?), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.FromSeconds(0), OnPropertyChanged));

    public static readonly DependencyProperty SelectionLastPointProperty
        = DependencyProperty.Register(nameof(SelectionLastPoint), typeof(TimeSpan?), typeof(PlayIndicator), new UIPropertyMetadata(TimeSpan.FromSeconds(0), OnPropertyChanged));

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

    public TimeSpan TotalDuration
    {
        get => (TimeSpan)GetValue(TotalDurationProperty);
        set => SetValue(TotalDurationProperty, value);
    }

    public TimeSpan? SelectionFirstPoint
    {
        get => (TimeSpan?)GetValue(SelectionFirstPointProperty);
        set => SetValue(SelectionFirstPointProperty, value);
    }

    public TimeSpan? SelectionLastPoint
    {
        get => (TimeSpan?)GetValue(SelectionLastPointProperty);
        set => SetValue(SelectionLastPointProperty, value);
    }

    public PlayIndicator()
    {
        InitializeComponent();
    }
    
    private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PlayIndicator playIndicator)
        {
            playIndicator.UpdatePlayIndicator();
        }
    }

    private void UpdatePlayIndicator()
    {
        // Update the play position bar
        {
            PlayPositionPoly.Points.Clear();

            TimeSpan clampedPosition = TimeSpan.FromSeconds(Math.Clamp(PlayPosition.TotalSeconds, 0, TotalDuration.TotalSeconds));

            double xpos = PlayIndicatorCanvas.ActualWidth * clampedPosition / TotalDuration;

            PlayPositionPoly.Points.Clear();
            PlayPositionPoly.Points.Add(new(xpos, 0));
            PlayPositionPoly.Points.Add(new(xpos, PlayIndicatorCanvas.ActualHeight));
            PlayPositionPoly.Points.Add(new(xpos + 1, PlayIndicatorCanvas.ActualHeight));
            PlayPositionPoly.Points.Add(new(xpos + 1, 0));
        }

        // Now update the visible sample area
        {
            PlayAreaPoly.Points.Clear();

            TimeSpan clampedStart = TimeSpan.FromSeconds(Math.Clamp(RenderStart.TotalSeconds, 0, TotalDuration.TotalSeconds));
            TimeSpan clampedEnd = TimeSpan.FromSeconds(Math.Clamp((RenderStart + RenderDuration).TotalSeconds, 0, TotalDuration.TotalSeconds));

            double xposStart = PlayIndicatorCanvas.ActualWidth * clampedStart / TotalDuration;
            double xposEnd = PlayIndicatorCanvas.ActualWidth * clampedEnd / TotalDuration;

            PlayAreaPoly.Points.Clear();
            PlayAreaPoly.Points.Add(new(xposStart, 0));
            PlayAreaPoly.Points.Add(new(xposStart, PlayIndicatorCanvas.ActualHeight));
            PlayAreaPoly.Points.Add(new(xposEnd, PlayIndicatorCanvas.ActualHeight));
            PlayAreaPoly.Points.Add(new(xposEnd, 0));
        }

        // Now update the selection area
        {
            if (SelectionFirstPoint == null || SelectionLastPoint == null)
                return;

            (var selectionStart, var selectionEnd) = (SelectionFirstPoint, SelectionLastPoint);

            if (SelectionFirstPoint > SelectionLastPoint)
                (selectionStart, selectionEnd) = (selectionEnd, selectionStart);

            TimeSpan clampedStart = TimeSpan.FromSeconds(Math.Clamp(selectionStart.Value.TotalSeconds, 0, TotalDuration.TotalSeconds));
            TimeSpan clampedEnd = TimeSpan.FromSeconds(Math.Clamp(selectionEnd.Value.TotalSeconds, 0, TotalDuration.TotalSeconds));

            double xposStart = ActualWidth * clampedStart / TotalDuration;
            double xposEnd = ActualWidth * clampedEnd / TotalDuration;

            SelectionPoly.Points.Clear();
            SelectionPoly.Points.Add(new(xposStart, 0));
            SelectionPoly.Points.Add(new(xposStart, ActualHeight));
            SelectionPoly.Points.Add(new(xposEnd, ActualHeight));
            SelectionPoly.Points.Add(new(xposEnd, 0));
        }
    }

}
