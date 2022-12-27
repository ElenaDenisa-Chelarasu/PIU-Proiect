using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioView.GUI;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyChanged([CallerMemberName] string? name = default)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    protected void Change<T>(ref T field, T value, [CallerMemberName] string? name = default)
    {
        field = value;
        NotifyChanged(name);
    }
}
