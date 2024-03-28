// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.Buffers.Text.FormattingHelpers
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Runtime.CompilerServices;

namespace System.Buffers.Text
{
    internal static class FormattingHelpers
    {
        internal const string HexTableLower = "0123456789abcdef";
        internal const string HexTableUpper = "0123456789ABCDEF";

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetSymbolOrDefault(in StandardFormat format, char defaultSymbol)
        {
            char ch = format.Symbol;
            if (ch == char.MinValue && format.Precision == (byte)0)
                ch = defaultSymbol;
            return ch;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillWithAsciiZeros(Span<byte> buffer)
        {
            for (int index = 0; index < buffer.Length; ++index)
                buffer[index] = (byte)48;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteHexByte(byte value, Span<byte> buffer, int startingIndex = 0, HexCasing casing = HexCasing.Uppercase)
        {
            uint num1 = (uint)((((int)value & 240) << 4) + ((int)value & 15) - 35209);
            uint num2 = (uint)((HexCasing)((int)((uint)(-(int)num1 & 28784) >> 4) + (int)num1 + 47545) | casing);
            buffer[startingIndex + 1] = (byte)num2;
            buffer[startingIndex] = (byte)(num2 >> 8);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDigits(ulong value, Span<byte> buffer)
        {
            for (int index = buffer.Length - 1; index >= 1; --index)
            {
                ulong num = 48UL + value;
                value /= 10UL;
                buffer[index] = (byte)(num - value * 10UL);
            }
            buffer[0] = (byte)(48UL + value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDigitsWithGroupSeparator(ulong value, Span<byte> buffer)
        {
            int num1 = 0;
            for (int index = buffer.Length - 1; index >= 1; --index)
            {
                ulong num2 = 48UL + value;
                value /= 10UL;
                buffer[index] = (byte)(num2 - value * 10UL);
                if (num1 == 2)
                {
                    buffer[--index] = (byte)44;
                    num1 = 0;
                }
                else
                    ++num1;
            }
            buffer[0] = (byte)(48UL + value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDigits(uint value, Span<byte> buffer)
        {
            for (int index = buffer.Length - 1; index >= 1; --index)
            {
                uint num = 48U + value;
                value /= 10U;
                buffer[index] = (byte)(num - value * 10U);
            }
            buffer[0] = (byte)(48U + value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFourDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
        {
            uint num1 = 48U + value;
            value /= 10U;
            buffer[startingIndex + 3] = (byte)(num1 - value * 10U);
            uint num2 = 48U + value;
            value /= 10U;
            buffer[startingIndex + 2] = (byte)(num2 - value * 10U);
            uint num3 = 48U + value;
            value /= 10U;
            buffer[startingIndex + 1] = (byte)(num3 - value * 10U);
            buffer[startingIndex] = (byte)(48U + value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteTwoDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
        {
            uint num = 48U + value;
            value /= 10U;
            buffer[startingIndex + 1] = (byte)(num - value * 10U);
            buffer[startingIndex] = (byte)(48U + value);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DivMod(ulong numerator, ulong denominator, out ulong modulo)
        {
            ulong num = numerator / denominator;
            modulo = numerator - num * denominator;
            return num;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DivMod(uint numerator, uint denominator, out uint modulo)
        {
            uint num = numerator / denominator;
            modulo = numerator - num * denominator;
            return num;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDecimalTrailingZeros(uint value, out uint valueWithoutTrailingZeros)
        {
            int num1 = 0;
            if (value != 0U)
            {
                while (true)
                {
                    uint modulo;
                    uint num2 = DivMod(value, 10U, out modulo);
                    if (modulo == 0U)
                    {
                        value = num2;
                        ++num1;
                    }
                    else
                        break;
                }
            }
            valueWithoutTrailingZeros = value;
            return num1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(ulong value)
        {
            int num1 = 1;
            uint num2;
            if (value >= 10000000UL)
            {
                if (value >= 100000000000000UL)
                {
                    num2 = (uint)(value / 100000000000000UL);
                    num1 += 14;
                }
                else
                {
                    num2 = (uint)(value / 10000000UL);
                    num1 += 7;
                }
            }
            else
                num2 = (uint)value;
            if (num2 >= 10U)
            {
                if (num2 < 100U)
                    ++num1;
                else if (num2 < 1000U)
                    num1 += 2;
                else if (num2 < 10000U)
                    num1 += 3;
                else if (num2 < 100000U)
                    num1 += 4;
                else if (num2 < 1000000U)
                    num1 += 5;
                else
                    num1 += 6;
            }
            return num1;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(uint value)
        {
            int num = 1;
            if (value >= 100000U)
            {
                value /= 100000U;
                num += 5;
            }
            if (value >= 10U)
            {
                if (value < 100U)
                    ++num;
                else if (value < 1000U)
                    num += 2;
                else if (value < 10000U)
                    num += 3;
                else
                    num += 4;
            }
            return num;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountHexDigits(ulong value)
        {
            int num = 1;
            if (value > uint.MaxValue)
            {
                num += 8;
                value >>= 32;
            }
            if (value > ushort.MaxValue)
            {
                num += 4;
                value >>= 16;
            }
            if (value > byte.MaxValue)
            {
                num += 2;
                value >>= 8;
            }
            if (value > 15UL)
                ++num;
            return num;
        }

        public enum HexCasing : uint
        {
            Uppercase = 0,
            Lowercase = 8224, // 0x00002020
        }
    }
}
