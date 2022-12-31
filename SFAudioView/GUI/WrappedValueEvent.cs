using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioView.GUI;

public class WrappedValueEvent<T> : EventArgs
{
    public WrappedValueEvent(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
