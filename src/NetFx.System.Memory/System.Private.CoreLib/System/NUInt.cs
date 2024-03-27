// Decompiled with JetBrains decompiler
// Type: System.NUInt
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System;
using System.Runtime.CompilerServices;

namespace System
{
    internal struct NUInt
    {
        private readonly unsafe void* _value;

        private unsafe NUInt(uint value)
        {
            _value = (void*)value;
        }

        private unsafe NUInt(ulong value)
        {
            _value = (void*)value;
        }

        public static implicit operator NUInt(uint value)
        {
            return new NUInt(value);
        }

        public static unsafe implicit operator IntPtr(NUInt value)
        {
            return (IntPtr)value._value;
        }

        public static explicit operator NUInt(int value)
        {
            return new NUInt((uint)value);
        }

        public static unsafe explicit operator void*(NUInt value)
        {
            return value._value;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static NUInt operator *(NUInt left, NUInt right)
        {
            if (sizeof(IntPtr) != 4)
            {
                return new NUInt((ulong)left._value * (ulong)right._value);
            }

            return new NUInt((uint)((int)left._value * (int)right._value));
        }

        // NetFx: Added so we can pass to Unsafe
        public static unsafe implicit operator nuint(NUInt value)
        {
            return (nuint)value._value;
        }
    }
}
