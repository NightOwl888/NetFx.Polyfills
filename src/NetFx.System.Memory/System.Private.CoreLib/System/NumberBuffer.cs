// Decompiled with JetBrains decompiler
// Type: System.NumberBuffer
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    internal ref struct NumberBuffer
    {
        public int Scale;
        public bool IsNegative;
        public const int BufferSize = 51;
        private byte _b0;
#pragma warning disable IDE0044, IDE0051, CS0169 // Remove unused private members
        private byte _b1;
        private byte _b2;
        private byte _b3;
        private byte _b4;
        private byte _b5;
        private byte _b6;
        private byte _b7;
        private byte _b8;
        private byte _b9;
        private byte _b10;
        private byte _b11;
        private byte _b12;
        private byte _b13;
        private byte _b14;
        private byte _b15;
        private byte _b16;
        private byte _b17;
        private byte _b18;
        private byte _b19;
        private byte _b20;
        private byte _b21;
        private byte _b22;
        private byte _b23;
        private byte _b24;
        private byte _b25;
        private byte _b26;
        private byte _b27;
        private byte _b28;
        private byte _b29;
        private byte _b30;
        private byte _b31;
        private byte _b32;
        private byte _b33;
        private byte _b34;
        private byte _b35;
        private byte _b36;
        private byte _b37;
        private byte _b38;
        private byte _b39;
        private byte _b40;
        private byte _b41;
        private byte _b42;
        private byte _b43;
        private byte _b44;
        private byte _b45;
        private byte _b46;
        private byte _b47;
        private byte _b48;
        private byte _b49;
        private byte _b50;
#pragma warning restore IDE0044, IDE0051, CS0169 // Remove unused private members

        public unsafe Span<byte> Digits
        {
            get
            {
                return new Span<byte>(Unsafe.AsPointer(ref _b0), 51);
            }
        }

        public unsafe byte* UnsafeDigits
        {
            get
            {
                return (byte*)Unsafe.AsPointer(ref _b0);
            }
        }

        public int NumDigits
        {
            get
            {
                return Digits.IndexOf<byte>(0);
            }
        }

        public bool HasNonZeroTail;
        public int DigitsCount;

        [Conditional("DEBUG")]
        public void CheckConsistency()
        {
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            stringBuilder.Append('"');
            Span<byte> digits = Digits;
            for (int index = 0; index < 51; ++index)
            {
                byte num = digits[index];
                if (num != (byte)0)
                    stringBuilder.Append((char)num);
                else
                    break;
            }
            stringBuilder.Append('"');
            stringBuilder.Append(", Scale = " + Scale);
            stringBuilder.Append(", IsNegative   = " + IsNegative.ToString());
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }
}
