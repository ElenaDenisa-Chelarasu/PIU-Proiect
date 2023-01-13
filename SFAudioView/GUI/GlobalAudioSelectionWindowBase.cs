using SFAudioView.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SFAudioView.GUI;

public class GlobalAudioSelectionWindowBase : Window
{
    // This is needed because you can't use dependency properties
    // defined in .xaml.cs from the .xaml of the same class
    // It's a bad workaround, but I haven't found anything better

    public static readonly DependencyProperty SelectionStateProperty
        = DependencyProperty.Register(nameof(SelectionState), typeof(SelectionState), typeof(GlobalAudioSelectionWindowBase), new UIPropertyMetadata(null, OnPropertyChanged));

    public event EventHandler<WrappedValueEvent<SelectionState?>>? SelectionStateUpdated;

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlobalAudioSelectionWindowBase window && e.Property == SelectionStateProperty)
        {
            window.SelectionStateUpdated?.Invoke(window, new WrappedValueEvent<SelectionState?>((SelectionState?)e.NewValue));
        }
    }

    public SelectionState? SelectionState
    {
        get => (SelectionState?)GetValue(SelectionStateProperty);
        set => SetValue(SelectionStateProperty, value);
    }
}
