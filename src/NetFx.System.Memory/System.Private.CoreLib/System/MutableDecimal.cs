// Decompiled with JetBrains decompiler
// Type: System.MutableDecimal
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

namespace System
{
    internal struct MutableDecimal
    {
        public uint Flags;
        public uint High;
        public uint Low;
        public uint Mid;
        private const uint SignMask = 2147483648;
        private const uint ScaleMask = 16711680;
        private const int ScaleShift = 16;

        public bool IsNegative
        {
            get
            {
                return (Flags & SignMask) > 0U;
            }
            set
            {
                Flags = (uint)((int)Flags & SignMask | (value ? SignMask : 0));
            }
        }

        public int Scale
        {
            get
            {
                return (int)(byte)(Flags >> 16);
            }
            set
            {
                Flags = (uint)((int)Flags & -16711681 | value << 16);
            }
        }
    }
}
