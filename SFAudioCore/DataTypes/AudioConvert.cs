using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SFAudioCore.DataTypes;

public static class AudioConvert
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Convert16ToFloat(ReadOnlySpan<short> from, Span<float> to)
    {
        if (from.Length != to.Length)
            throw new InvalidOperationException("Spans must be of the same size");

        unsafe
        {
            fixed (short* sPtr = from)
            {
                fixed (float* fPtr = to)
                {
                    int count = from.Length;
                    while (--count >= 0)
                    {
                        * (fPtr + count) = *(sPtr + count) / 32768f;
                    }
                }
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertFloatTo16(ReadOnlySpan<float> from, Span<short> to)
    {
        if (from.Length != to.Length)
            throw new InvalidOperationException("Spans must be of the same size");

        unsafe
        {
            fixed (float* fPtr = from)
            {
                fixed (short* sPtr = to)
                {
                    int count = from.Length;
                    while (--count >= 0)
                    {
                        *(sPtr + count) = (short)Math.Clamp(*(fPtr + count) * (short.MaxValue + 1), short.MinValue, short.MaxValue);
                    }
                }
            }
        }
    }
}
