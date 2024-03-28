// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.Buffers.Text.Utf8Formatter
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers.Text
{
    /// <summary>
    /// Provides static methods to format common data types as Utf8 strings.
    /// </summary>
    public static class Utf8Formatter
    {
        private static readonly uint[] DayAbbreviations = new uint[7]
        {
            7238995U,
            7237453U,
            6649172U,
            6579543U,
            7694420U,
            6910534U,
            7627091U
        };
        private static readonly uint[] DayAbbreviationsLowercase = new uint[7]
        {
            7239027U,
            7237485U,
            6649204U,
            6579575U,
            7694452U,
            6910566U,
            7627123U
        };
        private static readonly uint[] MonthAbbreviations = new uint[12]
        {
            7233866U,
            6448454U,
            7496013U,
            7499841U,
            7954765U,
            7238986U,
            7107914U,
            6780225U,
            7365971U,
            7627599U,
            7761742U,
            6513988U
        };
        private static readonly uint[] MonthAbbreviationsLowercase = new uint[12]
        {
            7233898U,
            6448486U,
            7496045U,
            7499873U,
            7954797U,
            7239018U,
            7107946U,
            6780257U,
            7366003U,
            7627631U,
            7761774U,
            6514020U
        };
        private const byte TimeMarker = 84;
        private const byte UtcMarker = 90;
        private const byte GMT1 = 71;
        private const byte GMT2 = 77;
        private const byte GMT3 = 84;
        private const byte GMT1Lowercase = 103;
        private const byte GMT2Lowercase = 109;
        private const byte GMT3Lowercase = 116;
        private const byte OpenBrace = 123;
        private const byte CloseBrace = 125;
        private const byte OpenParen = 40;
        private const byte CloseParen = 41;
        private const byte Dash = 45;

        /// <summary>
        /// Formats a Boolean as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G (default)   True/False
        ///     l             true/false
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(bool value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            char symbolOrDefault = FormattingHelpers.GetSymbolOrDefault(in format, 'G');
            if (value)
            {
                switch (symbolOrDefault)
                {
                    case 'G':
                        if (BinaryPrimitives.TryWriteUInt32BigEndian(destination, 1416787301U))
                            break;
                        goto BufferTooSmall;
                    case 'l':
                        if (!BinaryPrimitives.TryWriteUInt32BigEndian(destination, 1953658213U))
                            goto BufferTooSmall;
                        else
                            break;
                    default:
                        goto BadFormat;
                }
                bytesWritten = 4;
                return true;
            }
            switch (symbolOrDefault)
            {
                case 'G':
                    if (4U < (uint)destination.Length)
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(destination, 1180789875U);
                        break;
                    }
                    goto BufferTooSmall;
                case 'l':
                    if (4U < (uint)destination.Length)
                    {
                        BinaryPrimitives.WriteUInt32BigEndian(destination, 1717660787U);
                        break;
                    }
                    goto BufferTooSmall;
                default:
                    goto BadFormat;
            }
            destination[4] = (byte)101;
            bytesWritten = 5;
            return true;
        BufferTooSmall:
            bytesWritten = 0;
            return false;
        BadFormat:
            return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
        }

        /// <summary>
        /// Formats a DateTimeOffset as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds.
        /// </returns>
        /// <exceptions>
        /// <remarks>
        /// Formats supported:
        ///     default       05/25/2017 10:30:15 -08:00
        ///     G             05/25/2017 10:30:15
        ///     R             Tue, 03 Jan 2017 08:08:05 GMT       (RFC 1123)
        ///     l             tue, 03 jan 2017 08:08:05 gmt       (Lowercase RFC 1123)
        ///     O             2017-06-12T05:30:45.7680000-07:00   (Round-trippable)
        /// </remarks>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(DateTimeOffset value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            TimeSpan offset = Utf8Constants.s_nullUtcOffset;
            char ch = format.Symbol;
            if (format.IsDefault)
            {
                ch = 'G';
                offset = value.Offset;
            }
            switch (ch)
            {
                case 'G':
                    return TryFormatDateTimeG(value.DateTime, offset, destination, out bytesWritten);
                case 'O':
                    return TryFormatDateTimeO(value.DateTime, value.Offset, destination, out bytesWritten);
                case 'R':
                    return TryFormatDateTimeR(value.UtcDateTime, destination, out bytesWritten);
                case 'l':
                    return TryFormatDateTimeL(value.UtcDateTime, destination, out bytesWritten);
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        /// <summary>
        /// Formats a DateTime as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G  (default)  05/25/2017 10:30:15
        ///     R             Tue, 03 Jan 2017 08:08:05 GMT       (RFC 1123)
        ///     l             tue, 03 jan 2017 08:08:05 gmt       (Lowercase RFC 1123)
        ///     O             2017-06-12T05:30:45.7680000-07:00   (Round-trippable)
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(DateTime value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            switch (FormattingHelpers.GetSymbolOrDefault(in format, 'G'))
            {
                case 'G':
                    return TryFormatDateTimeG(value, Utf8Constants.s_nullUtcOffset, destination, out bytesWritten);
                case 'O':
                    return TryFormatDateTimeO(value, Utf8Constants.s_nullUtcOffset, destination, out bytesWritten);
                case 'R':
                    return TryFormatDateTimeR(value, destination, out bytesWritten);
                case 'l':
                    return TryFormatDateTimeL(value, destination, out bytesWritten);
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        private static bool TryFormatDateTimeG(DateTime value, TimeSpan offset, Span<byte> destination, out int bytesWritten)
        {
            int num1 = 19;
            if (offset != Utf8Constants.s_nullUtcOffset)
                num1 += 7;
            if (destination.Length < num1)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num1;
            byte num2 = destination[18];
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Month, destination, 0);
            destination[2] = (byte)47;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 3);
            destination[5] = (byte)47;
            FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 6);
            destination[10] = (byte)32;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 11);
            destination[13] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 14);
            destination[16] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 17);
            if (offset != Utf8Constants.s_nullUtcOffset)
            {
                byte num3;
                if (offset < new TimeSpan())
                {
                    num3 = (byte)45;
                    offset = TimeSpan.FromTicks(-offset.Ticks);
                }
                else
                    num3 = (byte)43;
                FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Minutes, destination, 24);
                destination[23] = (byte)58;
                FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Hours, destination, 21);
                destination[20] = num3;
                destination[19] = (byte)32;
            }
            return true;
        }

        private static bool TryFormatDateTimeO(DateTime value, TimeSpan offset, Span<byte> destination, out int bytesWritten)
        {
            int num1 = 27;
            DateTimeKind dateTimeKind = DateTimeKind.Local;
            if (offset == Utf8Constants.s_nullUtcOffset)
            {
                dateTimeKind = value.Kind;
                switch (dateTimeKind)
                {
                    case DateTimeKind.Utc:
                        ++num1;
                        break;
                    case DateTimeKind.Local:
                        offset = TimeZoneInfo.Local.GetUtcOffset(value);
                        num1 += 6;
                        break;
                }
            }
            else
                num1 += 6;
            if (destination.Length < num1)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num1;
            byte num2 = destination[26];
            FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 0);
            destination[4] = (byte)45;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Month, destination, 5);
            destination[7] = (byte)45;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 8);
            destination[10] = (byte)84;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 11);
            destination[13] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 14);
            destination[16] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 17);
            destination[19] = (byte)46;
            FormattingHelpers.WriteDigits((uint)((ulong)value.Ticks % 10000000UL), destination.Slice(20, 7));
            switch (dateTimeKind)
            {
                case DateTimeKind.Utc:
                    destination[27] = (byte)90;
                    break;
                case DateTimeKind.Local:
                    byte num3;
                    if (offset < new TimeSpan())
                    {
                        num3 = (byte)45;
                        offset = TimeSpan.FromTicks(-offset.Ticks);
                    }
                    else
                        num3 = (byte)43;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Minutes, destination, 31);
                    destination[30] = (byte)58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Hours, destination, 28);
                    destination[27] = num3;
                    break;
            }
            return true;
        }

        private static bool TryFormatDateTimeR(DateTime value, Span<byte> destination, out int bytesWritten)
        {
            if (28U >= (uint)destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            uint dayAbbreviation = DayAbbreviations[(int)value.DayOfWeek];
            destination[0] = (byte)dayAbbreviation;
            uint num1 = dayAbbreviation >> 8;
            destination[1] = (byte)num1;
            uint num2 = num1 >> 8;
            destination[2] = (byte)num2;
            destination[3] = (byte)44;
            destination[4] = (byte)32;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 5);
            destination[7] = (byte)32;
            uint monthAbbreviation = MonthAbbreviations[value.Month - 1];
            destination[8] = (byte)monthAbbreviation;
            uint num3 = monthAbbreviation >> 8;
            destination[9] = (byte)num3;
            uint num4 = num3 >> 8;
            destination[10] = (byte)num4;
            destination[11] = (byte)32;
            FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 12);
            destination[16] = (byte)32;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 17);
            destination[19] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 20);
            destination[22] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 23);
            destination[25] = (byte)32;
            destination[26] = (byte)71;
            destination[27] = (byte)77;
            destination[28] = (byte)84;
            bytesWritten = 29;
            return true;
        }

        private static bool TryFormatDateTimeL(DateTime value, Span<byte> destination, out int bytesWritten)
        {
            if (28U >= (uint)destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            uint num1 = Utf8Formatter.DayAbbreviationsLowercase[(int)value.DayOfWeek];
            destination[0] = (byte)num1;
            uint num2 = num1 >> 8;
            destination[1] = (byte)num2;
            uint num3 = num2 >> 8;
            destination[2] = (byte)num3;
            destination[3] = (byte)44;
            destination[4] = (byte)32;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 5);
            destination[7] = (byte)32;
            uint num4 = Utf8Formatter.MonthAbbreviationsLowercase[value.Month - 1];
            destination[8] = (byte)num4;
            uint num5 = num4 >> 8;
            destination[9] = (byte)num5;
            uint num6 = num5 >> 8;
            destination[10] = (byte)num6;
            destination[11] = (byte)32;
            FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 12);
            destination[16] = (byte)32;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 17);
            destination[19] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 20);
            destination[22] = (byte)58;
            FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 23);
            destination[25] = (byte)32;
            destination[26] = (byte)103;
            destination[27] = (byte)109;
            destination[28] = (byte)116;
            bytesWritten = 29;
            return true;
        }

        /// <summary>
        /// Formats a Decimal as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g  (default)  
        ///     F/f             12.45       Fixed point
        ///     E/e             1.245000e1  Exponential
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(decimal value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            if (format.IsDefault)
                format = (StandardFormat)'G';
            switch (format.Symbol)
            {
                case 'E':
                case 'e':
                    NumberBuffer number1 = new NumberBuffer();
                    Number.DecimalToNumber(value, ref number1);
                    byte precision1 = format.Precision == byte.MaxValue ? (byte)6 : format.Precision;
                    Number.RoundNumber(ref number1, (int)precision1 + 1);
                    return Utf8Formatter.TryFormatDecimalE(ref number1, destination, out bytesWritten, precision1, (byte)format.Symbol);
                case 'F':
                case 'f':
                    NumberBuffer number2 = new NumberBuffer();
                    Number.DecimalToNumber(value, ref number2);
                    byte precision2 = format.Precision == byte.MaxValue ? (byte)2 : format.Precision;
                    Number.RoundNumber(ref number2, number2.Scale + (int)precision2);
                    return Utf8Formatter.TryFormatDecimalF(ref number2, destination, out bytesWritten, precision2);
                case 'G':
                case 'g':
                    if (format.Precision != byte.MaxValue)
                        throw new NotSupportedException(SR.Argument_GWithPrecisionNotSupported);
                    NumberBuffer number3 = new NumberBuffer();
                    Number.DecimalToNumber(value, ref number3);
                    if (number3.Digits[0] == (byte)0)
                        number3.IsNegative = false;
                    return TryFormatDecimalG(ref number3, destination, out bytesWritten);
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        private static bool TryFormatDecimalE(ref NumberBuffer number, Span<byte> destination, out int bytesWritten, byte precision, byte exponentSymbol)
        {
            int scale = number.Scale;
            ReadOnlySpan<byte> digits = (ReadOnlySpan<byte>)number.Digits;
            int num1 = (number.IsNegative ? 1 : 0) + 1 + (precision == (byte)0 ? 0 : (int)precision + 1) + 2 + 3;
            if (destination.Length < num1)
            {
                bytesWritten = 0;
                return false;
            }
            int num2 = 0;
            int index1 = 0;
            if (number.IsNegative)
                destination[num2++] = (byte)45;
            byte num3 = digits[index1];
            int num4;
            int num5;
            if (num3 == (byte)0)
            {
                ref Span<byte> local = ref destination;
                int index2 = num2;
                num4 = index2 + 1;
                local[index2] = (byte)48;
                num5 = 0;
            }
            else
            {
                ref Span<byte> local = ref destination;
                int index2 = num2;
                num4 = index2 + 1;
                local[index2] = num3;
                ++index1;
                num5 = scale - 1;
            }
            if (precision > (byte)0)
            {
                destination[num4++] = (byte)46;
                for (int index2 = 0; index2 < (int)precision; ++index2)
                {
                    byte num6 = digits[index1];
                    if (num6 == (byte)0)
                    {
                        while (index2++ < (int)precision)
                            destination[num4++] = (byte)48;
                        break;
                    }
                    destination[num4++] = num6;
                    ++index1;
                }
            }
            ref Span<byte> local1 = ref destination;
            int index3 = num4;
            int num7 = index3 + 1;
            local1[index3] = exponentSymbol;
            int num8;
            if (num5 >= 0)
            {
                ref Span<byte> local2 = ref destination;
                int index2 = num7;
                num8 = index2 + 1;
                local2[index2] = (byte)43;
            }
            else
            {
                ref Span<byte> local2 = ref destination;
                int index2 = num7;
                num8 = index2 + 1;
                local2[index2] = (byte)45;
                num5 = -num5;
            }
            ref Span<byte> local3 = ref destination;
            int index4 = num8;
            int num9 = index4 + 1;
            local3[index4] = (byte)48;
            ref Span<byte> local4 = ref destination;
            int index5 = num9;
            int num10 = index5 + 1;
            local4[index5] = (byte)(num5 / 10 + 48);
            ref Span<byte> local5 = ref destination;
            int index6 = num10;
            int num11 = index6 + 1;
            local5[index6] = (byte)(num5 % 10 + 48);
            bytesWritten = num1;
            return true;
        }

        private static bool TryFormatDecimalF(ref NumberBuffer number, Span<byte> destination, out int bytesWritten, byte precision)
        {
            int scale = number.Scale;
            ReadOnlySpan<byte> digits = (ReadOnlySpan<byte>)number.Digits;
            int num1 = (number.IsNegative ? 1 : 0) + (scale <= 0 ? 1 : scale) + (precision == (byte)0 ? 0 : (int)precision + 1);
            if (destination.Length < num1)
            {
                bytesWritten = 0;
                return false;
            }
            int index1 = 0;
            int num2 = 0;
            if (number.IsNegative)
                destination[num2++] = (byte)45;
            if (scale <= 0)
            {
                destination[num2++] = (byte)48;
            }
            else
            {
                for (; index1 < scale; ++index1)
                {
                    byte num3 = digits[index1];
                    if (num3 == (byte)0)
                    {
                        int num4 = scale - index1;
                        for (int index2 = 0; index2 < num4; ++index2)
                            destination[num2++] = (byte)48;
                        break;
                    }
                    destination[num2++] = num3;
                }
            }
            if (precision > (byte)0)
            {
                ref Span<byte> local = ref destination;
                int index2 = num2;
                int num3 = index2 + 1;
                local[index2] = (byte)46;
                int num4 = 0;
                if (scale < 0)
                {
                    int num5 = Math.Min((int)precision, -scale);
                    for (int index3 = 0; index3 < num5; ++index3)
                        destination[num3++] = (byte)48;
                    num4 += num5;
                }
                for (; num4 < (int)precision; ++num4)
                {
                    byte num5 = digits[index1];
                    if (num5 == (byte)0)
                    {
                        while (num4++ < (int)precision)
                            destination[num3++] = (byte)48;
                        break;
                    }
                    destination[num3++] = num5;
                    ++index1;
                }
            }
            bytesWritten = num1;
            return true;
        }

        private static bool TryFormatDecimalG(ref NumberBuffer number, Span<byte> destination, out int bytesWritten)
        {
            int scale = number.Scale;
            ReadOnlySpan<byte> digits = (ReadOnlySpan<byte>)number.Digits;
            int numDigits = number.NumDigits;
            bool flag = scale < numDigits;
            int num1;
            if (flag)
            {
                num1 = numDigits + 1;
                if (scale <= 0)
                    num1 += 1 + -scale;
            }
            else
                num1 = scale <= 0 ? 1 : scale;
            if (number.IsNegative)
                ++num1;
            if (destination.Length < num1)
            {
                bytesWritten = 0;
                return false;
            }
            int index1 = 0;
            int num2 = 0;
            if (number.IsNegative)
                destination[num2++] = (byte)45;
            if (scale <= 0)
            {
                destination[num2++] = (byte)48;
            }
            else
            {
                for (; index1 < scale; ++index1)
                {
                    byte num3 = digits[index1];
                    if (num3 == (byte)0)
                    {
                        int num4 = scale - index1;
                        for (int index2 = 0; index2 < num4; ++index2)
                            destination[num2++] = (byte)48;
                        break;
                    }
                    destination[num2++] = num3;
                }
            }
            if (flag)
            {
                ref Span<byte> local1 = ref destination;
                int index2 = num2;
                int num3 = index2 + 1;
                local1[index2] = (byte)46;
                if (scale < 0)
                {
                    int num4 = -scale;
                    for (int index3 = 0; index3 < num4; ++index3)
                        destination[num3++] = (byte)48;
                }
                while (true)
                {
                    ref ReadOnlySpan<byte> local2 = ref digits;
                    int index3 = index1++;
                    byte num4;
                    if ((num4 = local2[index3]) != (byte)0)
                        destination[num3++] = num4;
                    else
                        break;
                }
            }
            bytesWritten = num1;
            return true;
        }

        /// <summary>
        /// Formats a Double as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g  (default)  
        ///     F/f             12.45       Fixed point
        ///     E/e             1.245000e1  Exponential
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(double value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatFloatingPoint(value, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats a Single as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g  (default)  
        ///     F/f             12.45       Fixed point
        ///     E/e             1.245000e1  Exponential
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(float value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatFloatingPoint(value, destination, out bytesWritten, format);
        }

        private static bool TryFormatFloatingPoint<T>(T value, Span<byte> destination, out int bytesWritten, StandardFormat format)
            where T : IFormattable
        {
            if (format.IsDefault)
                format = (StandardFormat)'G';
            switch (format.Symbol)
            {
                case 'E':
                case 'F':
                case 'e':
                case 'f':
                    string format1 = format.ToString();
                    string str = value.ToString(format1, CultureInfo.InvariantCulture);
                    int length = str.Length;
                    if (length > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    for (int index = 0; index < length; ++index)
                        destination[index] = (byte)str[index];
                    bytesWritten = length;
                    return true;
                case 'G':
                case 'g':
                    if (format.Precision != byte.MaxValue)
                        throw new NotSupportedException(SR.Argument_GWithPrecisionNotSupported);
                    goto case 'E';
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        /// <summary>
        /// Formats a Guid as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     D (default)     nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn
        ///     B               {nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn}
        ///     P               (nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn)
        ///     N               nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(Guid value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            int num1;
            switch (FormattingHelpers.GetSymbolOrDefault(in format, 'D'))
            {
                case 'B':
                    num1 = -2139260122;
                    break;
                case 'D':
                    num1 = -2147483612;
                    break;
                case 'N':
                    num1 = 32;
                    break;
                case 'P':
                    num1 = -2144786394;
                    break;
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
            if ((int)(byte)num1 > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = (int)(byte)num1;
            int num2 = num1 >> 8;
            if ((byte)num2 != (byte)0)
            {
                destination[0] = (byte)num2;
                destination = destination.Slice(1);
            }
            int num3 = num2 >> 8;
            DecomposedGuid decomposedGuid = default;
            decomposedGuid.Guid = value;
            byte num4 = destination[8];
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte03, destination, 0, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte02, destination, 2, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte01, destination, 4, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte00, destination, 6, FormattingHelpers.HexCasing.Lowercase);
            if (num3 < 0)
            {
                destination[8] = (byte)45;
                destination = destination.Slice(9);
            }
            else
                destination = destination.Slice(8);
            byte num5 = destination[4];
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte05, destination, 0, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte04, destination, 2, FormattingHelpers.HexCasing.Lowercase);
            if (num3 < 0)
            {
                destination[4] = (byte)45;
                destination = destination.Slice(5);
            }
            else
                destination = destination.Slice(4);
            byte num6 = destination[4];
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte07, destination, 0, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte06, destination, 2, FormattingHelpers.HexCasing.Lowercase);
            if (num3 < 0)
            {
                destination[4] = (byte)45;
                destination = destination.Slice(5);
            }
            else
                destination = destination.Slice(4);
            byte num7 = destination[4];
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte08, destination, 0, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte09, destination, 2, FormattingHelpers.HexCasing.Lowercase);
            if (num3 < 0)
            {
                destination[4] = (byte)45;
                destination = destination.Slice(5);
            }
            else
                destination = destination.Slice(4);
            byte num8 = destination[11];
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte10, destination, 0, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte11, destination, 2, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte12, destination, 4, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte13, destination, 6, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte14, destination, 8, FormattingHelpers.HexCasing.Lowercase);
            FormattingHelpers.WriteHexByte(decomposedGuid.Byte15, destination, 10, FormattingHelpers.HexCasing.Lowercase);
            if ((byte)num3 != (byte)0)
                destination[12] = (byte)num3;
            return true;
        }

        /// <summary>
        /// Formats a Byte as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(byte value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatUInt64(value, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats an SByte as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        [CLSCompliant(false)]
        public static bool TryFormat(sbyte value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatInt64((long)value, (ulong)byte.MaxValue, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats a Unt16 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        [CLSCompliant(false)]
        public static bool TryFormat(ushort value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatUInt64((ulong)value, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats an Int16 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(short value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatInt64((long)value, (ulong)ushort.MaxValue, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats a UInt32 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        [CLSCompliant(false)]
        public static bool TryFormat(uint value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatUInt64((ulong)value, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats an Int32 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(int value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatInt64((long)value, (ulong)uint.MaxValue, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats a UInt64 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        [CLSCompliant(false)]
        public static bool TryFormat(ulong value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatUInt64(value, destination, out bytesWritten, format);
        }

        /// <summary>
        /// Formats an Int64 as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds. 
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G/g (default)
        ///     D/d             32767  
        ///     N/n             32,767       
        ///     X/x             7fff
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(long value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            return TryFormatInt64(value, ulong.MaxValue, destination, out bytesWritten, format);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt64(long value, ulong mask, Span<byte> destination, out int bytesWritten, StandardFormat format)
        {
            if (format.IsDefault)
                return TryFormatInt64Default(value, destination, out bytesWritten);
            switch (format.Symbol)
            {
                case 'D':
                case 'd':
                    return TryFormatInt64D(value, format.Precision, destination, out bytesWritten);
                case 'G':
                case 'g':
                    if (format.HasPrecision)
                        throw new NotSupportedException(SR.Argument_GWithPrecisionNotSupported);
                    return TryFormatInt64D(value, format.Precision, destination, out bytesWritten);
                case 'N':
                case 'n':
                    return TryFormatInt64N(value, format.Precision, destination, out bytesWritten);
                case 'X':
                    return TryFormatUInt64X((ulong)value & mask, format.Precision, false, destination, out bytesWritten);
                case 'x':
                    return TryFormatUInt64X((ulong)value & mask, format.Precision, true, destination, out bytesWritten);
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt64D(long value, byte precision, Span<byte> destination, out int bytesWritten)
        {
            bool insertNegationSign = false;
            if (value < 0L)
            {
                insertNegationSign = true;
                value = -value;
            }
            return TryFormatUInt64D((ulong)value, precision, destination, insertNegationSign, out bytesWritten);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt64Default(long value, Span<byte> destination, out int bytesWritten)
        {
            if ((ulong)value < 10UL)
                return TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten);
            if (IntPtr.Size == 8)
                return TryFormatInt64MultipleDigits(value, destination, out bytesWritten);
            if (value <= (long)int.MaxValue && value >= (long)int.MinValue)
                return TryFormatInt32MultipleDigits((int)value, destination, out bytesWritten);
            return value <= 4294967295000000000L && value >= -4294967295000000000L
                ? (value >= 0L ? TryFormatUInt64LessThanBillionMaxUInt((ulong)value, destination, out bytesWritten) : TryFormatInt64MoreThanNegativeBillionMaxUInt(-value, destination, out bytesWritten))
                : (value >= 0L ? TryFormatUInt64MoreThanBillionMaxUInt((ulong)value, destination, out bytesWritten) : TryFormatInt64LessThanNegativeBillionMaxUInt(-value, destination, out bytesWritten));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt32Default(int value, Span<byte> destination, out int bytesWritten)
        {
            return (uint)value < 10U
                ? TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten)
                : TryFormatInt32MultipleDigits(value, destination, out bytesWritten);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt32MultipleDigits(int value, Span<byte> destination, out int bytesWritten)
        {
            if (value >= 0)
                return TryFormatUInt32MultipleDigits((uint)value, destination, out bytesWritten);
            value = -value;
            int length = FormattingHelpers.CountDigits((uint)value);
            if (length >= destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)45;
            bytesWritten = length + 1;
            FormattingHelpers.WriteDigits((uint)value, destination.Slice(1, length));
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt64MultipleDigits(long value, Span<byte> destination, out int bytesWritten)
        {
            if (value >= 0L)
                return Utf8Formatter.TryFormatUInt64MultipleDigits((ulong)value, destination, out bytesWritten);
            value = -value;
            int length = FormattingHelpers.CountDigits((ulong)value);
            if (length >= destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)45;
            bytesWritten = length + 1;
            FormattingHelpers.WriteDigits((ulong)value, destination.Slice(1, length));
            return true;
        }

        private static bool TryFormatInt64MoreThanNegativeBillionMaxUInt(long value, Span<byte> destination, out int bytesWritten)
        {
            uint num1 = (uint)((ulong)value / 1000000000UL);
            uint num2 = (uint)((ulong)value - (ulong)(num1 * 1000000000U));
            int length = FormattingHelpers.CountDigits(num1);
            int num3 = length + 9;
            if (num3 >= destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)45;
            bytesWritten = num3 + 1;
            FormattingHelpers.WriteDigits(num1, destination.Slice(1, length));
            FormattingHelpers.WriteDigits(num2, destination.Slice(length + 1, 9));
            return true;
        }

        private static bool TryFormatInt64LessThanNegativeBillionMaxUInt(long value, Span<byte> destination, out int bytesWritten)
        {
            ulong num1 = (ulong)value / 1000000000UL;
            uint num2 = (uint)(value - (long)num1 * 1000000000L);
            uint num3 = (uint)(num1 / 1000000000UL);
            uint num4 = (uint)(num1 - (ulong)(num3 * 1000000000U));
            int length = FormattingHelpers.CountDigits(num3);
            int num5 = length + 18;
            if (num5 >= destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)45;
            bytesWritten = num5 + 1;
            FormattingHelpers.WriteDigits(num3, destination.Slice(1, length));
            FormattingHelpers.WriteDigits(num4, destination.Slice(length + 1, 9));
            FormattingHelpers.WriteDigits(num2, destination.Slice(length + 1 + 9, 9));
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatInt64N(long value, byte precision, Span<byte> destination, out int bytesWritten)
        {
            bool insertNegationSign = false;
            if (value < 0L)
            {
                insertNegationSign = true;
                value = -value;
            }
            return TryFormatUInt64N((ulong)value, precision, destination, insertNegationSign, out bytesWritten);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt64(ulong value, Span<byte> destination, out int bytesWritten, StandardFormat format)
        {
            if (format.IsDefault)
                return TryFormatUInt64Default(value, destination, out bytesWritten);
            switch (format.Symbol)
            {
                case 'D':
                case 'd':
                    return TryFormatUInt64D(value, format.Precision, destination, false, out bytesWritten);
                case 'G':
                case 'g':
                    if (format.HasPrecision)
                        throw new NotSupportedException(SR.Argument_GWithPrecisionNotSupported);
                    return TryFormatUInt64D(value, format.Precision, destination, false, out bytesWritten);
                case 'N':
                case 'n':
                    return TryFormatUInt64N(value, format.Precision, destination, false, out bytesWritten);
                case 'X':
                    return TryFormatUInt64X(value, format.Precision, false, destination, out bytesWritten);
                case 'x':
                    return TryFormatUInt64X(value, format.Precision, true, destination, out bytesWritten);
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        private static bool TryFormatUInt64D(ulong value, byte precision, Span<byte> destination, bool insertNegationSign, out int bytesWritten)
        {
            int length = FormattingHelpers.CountDigits(value);
            int num1 = (precision == byte.MaxValue ? 0 : (int)precision) - length;
            if (num1 < 0)
                num1 = 0;
            int num2 = length + num1;
            if (insertNegationSign)
                ++num2;
            if (num2 > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num2;
            if (insertNegationSign)
            {
                destination[0] = (byte)45;
                destination = destination.Slice(1);
            }
            if (num1 > 0)
                FormattingHelpers.FillWithAsciiZeros(destination.Slice(0, num1));
            FormattingHelpers.WriteDigits(value, destination.Slice(num1, length));
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt64Default(ulong value, Span<byte> destination, out int bytesWritten)
        {
            if (value < 10UL)
                return TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten);
            if (IntPtr.Size == 8)
                return TryFormatUInt64MultipleDigits(value, destination, out bytesWritten);
            if (value <= (ulong)uint.MaxValue)
                return TryFormatUInt32MultipleDigits((uint)value, destination, out bytesWritten);
            return value <= 4294967295000000000UL
                ? TryFormatUInt64LessThanBillionMaxUInt(value, destination, out bytesWritten)
                : TryFormatUInt64MoreThanBillionMaxUInt(value, destination, out bytesWritten);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt32Default(uint value, Span<byte> destination, out int bytesWritten)
        {
            return value < 10U
                ? TryFormatUInt32SingleDigit(value, destination, out bytesWritten)
                : TryFormatUInt32MultipleDigits(value, destination, out bytesWritten);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt32SingleDigit(uint value, Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)(48U + value);
            bytesWritten = 1;
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt32MultipleDigits(uint value, Span<byte> destination, out int bytesWritten)
        {
            int length = FormattingHelpers.CountDigits(value);
            if (length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = length;
            FormattingHelpers.WriteDigits(value, destination.Slice(0, length));
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt64SingleDigit(ulong value, Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }
            destination[0] = (byte)(48UL + value);
            bytesWritten = 1;
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatUInt64MultipleDigits(ulong value, Span<byte> destination, out int bytesWritten)
        {
            int length = FormattingHelpers.CountDigits(value);
            if (length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = length;
            FormattingHelpers.WriteDigits(value, destination.Slice(0, length));
            return true;
        }

        private static bool TryFormatUInt64LessThanBillionMaxUInt(ulong value, Span<byte> destination, out int bytesWritten)
        {
            uint num1 = (uint)(value / 1000000000UL);
            uint num2 = (uint)(value - (ulong)(num1 * 1000000000U));
            int num3 = FormattingHelpers.CountDigits(num1);
            int num4 = num3 + 9;
            if (num4 > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num4;
            FormattingHelpers.WriteDigits(num1, destination.Slice(0, num3));
            FormattingHelpers.WriteDigits(num2, destination.Slice(num3, 9));
            return true;
        }

        private static bool TryFormatUInt64MoreThanBillionMaxUInt(ulong value, Span<byte> destination, out int bytesWritten)
        {
            ulong num1 = value / 1000000000UL;
            uint num2 = (uint)(value - num1 * 1000000000UL);
            uint num3 = (uint)(num1 / 1000000000UL);
            uint num4 = (uint)(num1 - (ulong)(num3 * 1000000000U));
            int num5 = FormattingHelpers.CountDigits(num3);
            int num6 = num5 + 18;
            if (num6 > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num6;
            FormattingHelpers.WriteDigits(num3, destination.Slice(0, num5));
            FormattingHelpers.WriteDigits(num4, destination.Slice(num5, 9));
            FormattingHelpers.WriteDigits(num2, destination.Slice(num5 + 9, 9));
            return true;
        }

        private static bool TryFormatUInt64N(ulong value, byte precision, Span<byte> destination, bool insertNegationSign, out int bytesWritten)
        {
            int num1 = FormattingHelpers.CountDigits(value);
            int num2 = (num1 - 1) / 3;
            int length = precision == byte.MaxValue ? 2 : (int)precision;
            int num3 = num1 + num2;
            if (length > 0)
                num3 += length + 1;
            if (insertNegationSign)
                ++num3;
            if (num3 > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = num3;
            if (insertNegationSign)
            {
                destination[0] = (byte)45;
                destination = destination.Slice(1);
            }
            FormattingHelpers.WriteDigitsWithGroupSeparator(value, destination.Slice(0, num1 + num2));
            if (length > 0)
            {
                destination[num1 + num2] = (byte)46;
                FormattingHelpers.FillWithAsciiZeros(destination.Slice(num1 + num2 + 1, length));
            }
            return true;
        }

        private static bool TryFormatUInt64X(ulong value, byte precision, bool useLower, Span<byte> destination, out int bytesWritten)
        {
            int val2 = FormattingHelpers.CountHexDigits(value);
            int index = precision == byte.MaxValue ? val2 : Math.Max((int)precision, val2);
            if (destination.Length < index)
            {
                bytesWritten = 0;
                return false;
            }
            bytesWritten = index;
            string str = useLower ? "0123456789abcdef" : "0123456789ABCDEF";
            while ((uint)--index < (uint)destination.Length)
            {
                destination[index] = (byte)str[(int)value & 15];
                value >>= 4;
            }
            return true;
        }

        /// <summary>
        /// Formats a TimeSpan as a UTF8 string.
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="destination">Buffer to write the UTF8-formatted value to</param>
        /// <param name="bytesWritten">Receives the length of the formatted text in bytes</param>
        /// <param name="format">The standard format to use</param>
        /// <returns>
        /// true for success. "bytesWritten" contains the length of the formatted text in bytes.
        /// false if buffer was too short. Iteratively increase the size of the buffer and retry until it succeeds.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     c/t/T (default) [-][d.]hh:mm:ss[.fffffff]              (constant format)
        ///     G               [-]d:hh:mm:ss.fffffff                  (general long)
        ///     g               [-][d:][h]h:mm:ss[.f[f[f[f[f[f[f]]]]]] (general short)
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryFormat(TimeSpan value, Span<byte> destination, out int bytesWritten, StandardFormat format = default)
        {
            char ch = FormattingHelpers.GetSymbolOrDefault(in format, 'c');
            switch (ch)
            {
                case 'G':
                case 'c':
                case 'g':
                    int num1 = 8;
                    long ticks = value.Ticks;
                    uint valueWithoutTrailingZeros;
                    ulong numerator1;
                    if (ticks < 0L && -ticks < 0L)
                    {
                        valueWithoutTrailingZeros = 4775808U;
                        numerator1 = 922337203685UL;
                    }
                    else
                    {
                        ulong modulo;
                        numerator1 = FormattingHelpers.DivMod((ulong)Math.Abs(value.Ticks), 10000000UL, out modulo);
                        valueWithoutTrailingZeros = (uint)modulo;
                    }
                    int length1 = 0;
                    switch (ch)
                    {
                        case 'G':
                            length1 = 7;
                            break;
                        case 'c':
                            if (valueWithoutTrailingZeros != 0U)
                            {
                                length1 = 7;
                                break;
                            }
                            break;
                        default:
                            if (valueWithoutTrailingZeros != 0U)
                            {
                                length1 = 7 - FormattingHelpers.CountDecimalTrailingZeros(valueWithoutTrailingZeros, out valueWithoutTrailingZeros);
                                break;
                            }
                            break;
                    }
                    if (length1 != 0)
                        num1 += length1 + 1;
                    ulong numerator2 = 0;
                    ulong modulo1 = 0;
                    if (numerator1 > 0UL)
                        numerator2 = FormattingHelpers.DivMod(numerator1, 60UL, out modulo1);
                    ulong num2 = 0;
                    ulong modulo2 = 0;
                    if (numerator2 > 0UL)
                        num2 = FormattingHelpers.DivMod(numerator2, 60UL, out modulo2);
                    uint num3 = 0;
                    uint modulo3 = 0;
                    if (num2 > 0UL)
                        num3 = FormattingHelpers.DivMod((uint)num2, 24U, out modulo3);
                    int length2 = 2;
                    if (modulo3 < 10U && ch == 'g')
                    {
                        --length2;
                        --num1;
                    }
                    int length3 = 0;
                    if (num3 == 0U)
                    {
                        if (ch == 'G')
                        {
                            num1 += 2;
                            length3 = 1;
                        }
                    }
                    else
                    {
                        length3 = FormattingHelpers.CountDigits(num3);
                        num1 += length3 + 1;
                    }
                    if (value.Ticks < 0L)
                        ++num1;
                    if (destination.Length < num1)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num1;
                    int start1 = 0;
                    if (value.Ticks < 0L)
                        destination[start1++] = (byte)45;
                    if (length3 > 0)
                    {
                        FormattingHelpers.WriteDigits(num3, destination.Slice(start1, length3));
                        int num4 = start1 + length3;
                        ref Span<byte> local = ref destination;
                        int index = num4;
                        start1 = index + 1;
                        local[index] = ch == 'c' ? (byte)46 : (byte)58;
                    }
                    FormattingHelpers.WriteDigits(modulo3, destination.Slice(start1, length2));
                    int num5 = start1 + length2;
                    ref Span<byte> local1 = ref destination;
                    int index1 = num5;
                    int start2 = index1 + 1;
                    local1[index1] = (byte)58;
                    FormattingHelpers.WriteDigits((uint)modulo2, destination.Slice(start2, 2));
                    int num6 = start2 + 2;
                    ref Span<byte> local2 = ref destination;
                    int index2 = num6;
                    int start3 = index2 + 1;
                    local2[index2] = (byte)58;
                    FormattingHelpers.WriteDigits((uint)modulo1, destination.Slice(start3, 2));
                    int num7 = start3 + 2;
                    if (length1 > 0)
                    {
                        ref Span<byte> local3 = ref destination;
                        int index3 = num7;
                        int start4 = index3 + 1;
                        local3[index3] = (byte)46;
                        FormattingHelpers.WriteDigits(valueWithoutTrailingZeros, destination.Slice(start4, length1));
                        int num4 = start4 + length1;
                    }
                    return true;
                case 'T':
                case 't':
                    ch = 'c';
                    goto case 'G';
                default:
                    return ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DecomposedGuid
        {
            [FieldOffset(0)]
            public Guid Guid;
            [FieldOffset(0)]
            public byte Byte00;
            [FieldOffset(1)]
            public byte Byte01;
            [FieldOffset(2)]
            public byte Byte02;
            [FieldOffset(3)]
            public byte Byte03;
            [FieldOffset(4)]
            public byte Byte04;
            [FieldOffset(5)]
            public byte Byte05;
            [FieldOffset(6)]
            public byte Byte06;
            [FieldOffset(7)]
            public byte Byte07;
            [FieldOffset(8)]
            public byte Byte08;
            [FieldOffset(9)]
            public byte Byte09;
            [FieldOffset(10)]
            public byte Byte10;
            [FieldOffset(11)]
            public byte Byte11;
            [FieldOffset(12)]
            public byte Byte12;
            [FieldOffset(13)]
            public byte Byte13;
            [FieldOffset(14)]
            public byte Byte14;
            [FieldOffset(15)]
            public byte Byte15;
        }
    }
}
