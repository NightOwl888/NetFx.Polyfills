using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    internal static partial class SpanHelpers
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(
            ReadOnlySpan<T> span,
            TComparable comparable)
            where TComparable : IComparable<T>
        {
            if (comparable == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparable);

            return BinarySearch(ref MemoryMarshal.GetReference(span), span.Length, comparable!);
        }

        public static int BinarySearch<T, TComparable>(
            ref T spanStart,
            int length,
            TComparable comparable)
            where TComparable : IComparable<T>
        {
            int num1 = 0;
            int num2 = length - 1;
            while (num1 <= num2)
            {
                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint.
                //       Saves one subtraction per loop compared to
                //       `int i = lo + ((hi - lo) >> 1);`
                int i = (int)((uint)(num2 + num1) >> 1);
                int num3 = comparable.CompareTo(Unsafe.Add(ref spanStart, i));
                if (num3 == 0)
                    return i;
                if (num3 > 0)
                    num1 = i + 1;
                else
                    num2 = i - 1;
            }
            return ~num1;
        }
    }
}
