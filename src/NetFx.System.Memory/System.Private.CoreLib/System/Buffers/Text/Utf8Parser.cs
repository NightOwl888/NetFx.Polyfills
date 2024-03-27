// Decompiled with JetBrains decompiler
// Type: System.Buffers.Text.Utf8Parser
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Buffers.Text
{
    /// <summary>
    /// Methods to parse common data types to Utf8 strings.
    /// </summary>
    public static class Utf8Parser
    {
        private static readonly int[] s_daysToMonth365 = new int[13]
        {
            0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
            304, 334, 365
        };

        private static readonly int[] s_daysToMonth366 = new int[13]
        {
            0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
            305, 335, 366
        };
        private const uint FlipCase = 32;
        private const uint NoFlipCase = 0;

        /// <summary>
        /// Parses a Boolean at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     G (default)   True/False
        ///     l             true/false
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryParse(ReadOnlySpan<byte> source, out bool value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat != char.MinValue && standardFormat != 'G' && standardFormat != 'l')
                return ThrowHelper.TryParseThrowFormatException<bool>(out value, out bytesConsumed);
            if (source.Length >= 4)
            {
                if ((source[0] == (byte)84 || source[0] == (byte)116) && (source[1] == (byte)82 || source[1] == (byte)114) && ((source[2] == (byte)85 || source[2] == (byte)117) && (source[3] == (byte)69 || source[3] == (byte)101)))
                {
                    bytesConsumed = 4;
                    value = true;
                    return true;
                }
                if (source.Length >= 5 && (source[0] == (byte)70 || source[0] == (byte)102) && ((source[1] == (byte)65 || source[1] == (byte)97) && (source[2] == (byte)76 || source[2] == (byte)108)) && ((source[3] == (byte)83 || source[3] == (byte)115) && (source[4] == (byte)69 || source[4] == (byte)101)))
                {
                    bytesConsumed = 5;
                    value = false;
                    return true;
                }
            }
            bytesConsumed = 0;
            value = false;
            return false;
        }

        /// <summary>
        /// Parses a DateTime at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     default       05/25/2017 10:30:15 -08:00
        ///     G             05/25/2017 10:30:15
        ///     R             Tue, 03 Jan 2017 08:08:05 GMT       (RFC 1123)
        ///     l             tue, 03 jan 2017 08:08:05 gmt       (Lowercase RFC 1123)
        ///     O             2017-06-12T05:30:45.7680000-07:00   (Round-trippable)
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryParse(ReadOnlySpan<byte> source, out DateTime value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'G':
                    return TryParseDateTimeG(source, out value, out DateTimeOffset _, out bytesConsumed);
                case 'O':
                    DateTimeOffset dateTimeOffset1;
                    DateTimeKind kind;
                    if (!TryParseDateTimeOffsetO(source, out dateTimeOffset1, out bytesConsumed, out kind))
                    {
                        value = new DateTime();
                        bytesConsumed = 0;
                        return false;
                    }
                    switch (kind)
                    {
                        case DateTimeKind.Utc:
                            value = dateTimeOffset1.UtcDateTime;
                            break;
                        case DateTimeKind.Local:
                            value = dateTimeOffset1.LocalDateTime;
                            break;
                        default:
                            value = dateTimeOffset1.DateTime;
                            break;
                    }
                    return true;
                case 'R':
                    DateTimeOffset dateTimeOffset2;
                    if (!TryParseDateTimeOffsetR(source, 0U, out dateTimeOffset2, out bytesConsumed))
                    {
                        value = new DateTime();
                        return false;
                    }
                    value = dateTimeOffset2.DateTime;
                    return true;
                case 'l':
                    DateTimeOffset dateTimeOffset3;
                    if (!TryParseDateTimeOffsetR(source, 32U, out dateTimeOffset3, out bytesConsumed))
                    {
                        value = new DateTime();
                        return false;
                    }
                    value = dateTimeOffset3.DateTime;
                    return true;
                default:
                    return ThrowHelper.TryParseThrowFormatException<DateTime>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses a DateTimeOffset at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, char standardFormat = default)
        {
            return standardFormat switch
            {
                char.MinValue => TryParseDateTimeOffsetDefault(source, out value, out bytesConsumed),
                'G' => TryParseDateTimeG(source, out DateTime _, out value, out bytesConsumed),
                'O' => TryParseDateTimeOffsetO(source, out value, out bytesConsumed, out DateTimeKind _),
                'R' => TryParseDateTimeOffsetR(source, 0U, out value, out bytesConsumed),
                'l' => TryParseDateTimeOffsetR(source, 32U, out value, out bytesConsumed),
                _ => ThrowHelper.TryParseThrowFormatException<DateTimeOffset>(out value, out bytesConsumed),
            };
        }

        private static bool TryParseDateTimeOffsetDefault(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed)
        {
            if (source.Length < 26)
            {
                bytesConsumed = 0;
                value = new DateTimeOffset();
                return false;
            }
            DateTime dateTime;
            if (!TryParseDateTimeG(source, out dateTime, out DateTimeOffset _, out int _))
            {
                bytesConsumed = 0;
                value = new DateTimeOffset();
                return false;
            }
            if (source[19] != (byte)32)
            {
                bytesConsumed = 0;
                value = new DateTimeOffset();
                return false;
            }
            byte num1 = source[20];
            switch (num1)
            {
                case 43:
                case 45:
                    uint num2 = (uint)source[21] - 48U;
                    uint num3 = (uint)source[22] - 48U;
                    if (num2 > 9U || num3 > 9U)
                    {
                        bytesConsumed = 0;
                        value = new DateTimeOffset();
                        return false;
                    }
                    int num4 = (int)num2 * 10 + (int)num3;
                    if (source[23] != (byte)58)
                    {
                        bytesConsumed = 0;
                        value = new DateTimeOffset();
                        return false;
                    }
                    uint num5 = (uint)source[24] - 48U;
                    uint num6 = (uint)source[25] - 48U;
                    if (num5 > 9U || num6 > 9U)
                    {
                        bytesConsumed = 0;
                        value = new DateTimeOffset();
                        return false;
                    }
                    int num7 = (int)num5 * 10 + (int)num6;
                    TimeSpan timeSpan = new TimeSpan(num4, num7, 0);
                    if (num1 == (byte)45)
                        timeSpan = -timeSpan;
                    if (!TryCreateDateTimeOffset(dateTime, num1 == (byte)45, num4, num7, out value))
                    {
                        bytesConsumed = 0;
                        value = new DateTimeOffset();
                        return false;
                    }
                    bytesConsumed = 26;
                    return true;
                default:
                    bytesConsumed = 0;
                    value = new DateTimeOffset();
                    return false;
            }
        }

        private static bool TryParseDateTimeG(ReadOnlySpan<byte> source, out DateTime value, out DateTimeOffset valueAsOffset, out int bytesConsumed)
        {
            if (source.Length < 19)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num1 = (uint)source[0] - 48U;
            uint num2 = (uint)source[1] - 48U;
            if (num1 > 9U || num2 > 9U)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int month = (int)num1 * 10 + (int)num2;
            if (source[2] != (byte)47)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num3 = (uint)source[3] - 48U;
            uint num4 = (uint)source[4] - 48U;
            if (num3 > 9U || num4 > 9U)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int day = (int)num3 * 10 + (int)num4;
            if (source[5] != (byte)47)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num5 = (uint)source[6] - 48U;
            uint num6 = (uint)source[7] - 48U;
            uint num7 = (uint)source[8] - 48U;
            uint num8 = (uint)source[9] - 48U;
            if (num5 > 9U || num6 > 9U || (num7 > 9U || num8 > 9U))
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int year = (int)num5 * 1000 + (int)num6 * 100 + (int)num7 * 10 + (int)num8;
            if (source[10] != (byte)32)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num9 = (uint)source[11] - 48U;
            uint num10 = (uint)source[12] - 48U;
            if (num9 > 9U || num10 > 9U)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int hour = (int)num9 * 10 + (int)num10;
            if (source[13] != (byte)58)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num11 = (uint)source[14] - 48U;
            uint num12 = (uint)source[15] - 48U;
            if (num11 > 9U || num12 > 9U)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int minute = (int)num11 * 10 + (int)num12;
            if (source[16] != (byte)58)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            uint num13 = (uint)source[17] - 48U;
            uint num14 = (uint)source[18] - 48U;
            if (num13 > 9U || num14 > 9U)
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            int second = (int)num13 * 10 + (int)num14;
            if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year, month, day, hour, minute, second, 0, out valueAsOffset))
            {
                bytesConsumed = 0;
                value = new DateTime();
                valueAsOffset = new DateTimeOffset();
                return false;
            }
            bytesConsumed = 19;
            value = valueAsOffset.DateTime;
            return true;
        }

        private static bool TryCreateDateTimeOffset(DateTime dateTime, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
        {
            if ((uint)offsetHours > 14U)
            {
                value = new DateTimeOffset();
                return false;
            }
            if ((uint)offsetMinutes > 59U)
            {
                value = new DateTimeOffset();
                return false;
            }
            if (offsetHours == 14 && offsetMinutes != 0)
            {
                value = new DateTimeOffset();
                return false;
            }
            long ticks = ((long)offsetHours * 3600L + (long)offsetMinutes * 60L) * 10000000L;
            if (offsetNegative)
                ticks = -ticks;
            try
            {
                value = new DateTimeOffset(dateTime.Ticks, new TimeSpan(ticks));
            }
            catch (ArgumentOutOfRangeException)
            {
                value = new DateTimeOffset();
                return false;
            }
            return true;
        }

        private static bool TryCreateDateTimeOffset(int year, int month, int day, int hour, int minute,
            int second, int fraction, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
        {
            DateTime dateTime;
            if (!TryCreateDateTime(year, month, day, hour, minute, second, fraction, DateTimeKind.Unspecified, out dateTime))
            {
                value = new DateTimeOffset();
                return false;
            }
            if (TryCreateDateTimeOffset(dateTime, offsetNegative, offsetHours, offsetMinutes, out value))
                return true;
            value = new DateTimeOffset();
            return false;
        }

        private static bool TryCreateDateTimeOffsetInterpretingDataAsLocalTime(int year, int month, int day, int hour, int minute,
            int second, int fraction, out DateTimeOffset value)
        {
            if (!TryCreateDateTime(year, month, day, hour, minute, second, fraction, DateTimeKind.Local, out DateTime dateTime))
            {
                value = new DateTimeOffset();
                return false;
            }
            try
            {
                value = new DateTimeOffset(dateTime);
            }
            catch (ArgumentOutOfRangeException)
            {
                value = new DateTimeOffset();
                return false;
            }
            return true;
        }

        private static bool TryCreateDateTime(int year, int month, int day, int hour, int minute,
            int second, int fraction, DateTimeKind kind, out DateTime value)
        {
            if (year == 0)
            {
                value = new DateTime();
                return false;
            }
            if ((uint)(month - 1) >= 12U)
            {
                value = new DateTime();
                return false;
            }
            uint num1 = (uint)(day - 1);
            if (num1 >= 28U && (long)num1 >= (long)DateTime.DaysInMonth(year, month))
            {
                value = new DateTime();
                return false;
            }
            if ((uint)hour > 23U)
            {
                value = new DateTime();
                return false;
            }
            if ((uint)minute > 59U)
            {
                value = new DateTime();
                return false;
            }
            if ((uint)second > 59U)
            {
                value = new DateTime();
                return false;
            }
            int[] numArray = DateTime.IsLeapYear(year) ? s_daysToMonth366 : s_daysToMonth365;
            int num2 = year - 1;
            long ticks = (long)(num2 * 365 + num2 / 4 - num2 / 100 + num2 / 400 + numArray[month - 1] + day - 1) * 864000000000L + (long)(hour * 3600 + minute * 60 + second) * 10000000L + (long)fraction;
            value = new DateTime(ticks, kind);
            return true;
        }

        private static bool TryParseDateTimeOffsetO(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, out DateTimeKind kind)
        {
            if (source.Length < 27)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num1 = (uint)source[0] - 48U;
            uint num2 = (uint)source[1] - 48U;
            uint num3 = (uint)source[2] - 48U;
            uint num4 = (uint)source[3] - 48U;
            if (num1 > 9U || num2 > 9U || (num3 > 9U || num4 > 9U))
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int year = (int)num1 * 1000 + (int)num2 * 100 + (int)num3 * 10 + (int)num4;
            if (source[4] != (byte)45)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num5 = (uint)source[5] - 48U;
            uint num6 = (uint)source[6] - 48U;
            if (num5 > 9U || num6 > 9U)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int month = (int)num5 * 10 + (int)num6;
            if (source[7] != (byte)45)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num7 = (uint)source[8] - 48U;
            uint num8 = (uint)source[9] - 48U;
            if (num7 > 9U || num8 > 9U)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int day = (int)num7 * 10 + (int)num8;
            if (source[10] != (byte)84)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num9 = (uint)source[11] - 48U;
            uint num10 = (uint)source[12] - 48U;
            if (num9 > 9U || num10 > 9U)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int hour = (int)num9 * 10 + (int)num10;
            if (source[13] != (byte)58)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num11 = (uint)source[14] - 48U;
            uint num12 = (uint)source[15] - 48U;
            if (num11 > 9U || num12 > 9U)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int minute = (int)num11 * 10 + (int)num12;
            if (source[16] != (byte)58)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num13 = (uint)source[17] - 48U;
            uint num14 = (uint)source[18] - 48U;
            if (num13 > 9U || num14 > 9U)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int second = (int)num13 * 10 + (int)num14;
            if (source[19] != (byte)46)
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            uint num15 = (uint)source[20] - 48U;
            uint num16 = (uint)source[21] - 48U;
            uint num17 = (uint)source[22] - 48U;
            uint num18 = (uint)source[23] - 48U;
            uint num19 = (uint)source[24] - 48U;
            uint num20 = (uint)source[25] - 48U;
            uint num21 = (uint)source[26] - 48U;
            if (num15 > 9U || num16 > 9U || (num17 > 9U || num18 > 9U) || (num19 > 9U || num20 > 9U || num21 > 9U))
            {
                value = new DateTimeOffset();
                bytesConsumed = 0;
                kind = DateTimeKind.Unspecified;
                return false;
            }
            int fraction = (int)num15 * 1000000 + (int)num16 * 100000 + (int)num17 * 10000 + (int)num18 * 1000 + (int)num19 * 100 + (int)num20 * 10 + (int)num21;
            byte num22 = source.Length <= 27 ? (byte)0 : source[27];
            switch (num22)
            {
                case 43:
                case 45:
                case 90:
                    if (num22 == (byte)90)
                    {
                        if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, fraction, false, 0, 0, out value))
                        {
                            value = new DateTimeOffset();
                            bytesConsumed = 0;
                            kind = DateTimeKind.Unspecified;
                            return false;
                        }
                        bytesConsumed = 28;
                        kind = DateTimeKind.Utc;
                        return true;
                    }
                    if (source.Length < 33)
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num23 = (uint)source[28] - 48U;
                    uint num24 = (uint)source[29] - 48U;
                    if (num23 > 9U || num24 > 9U)
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int offsetHours = (int)num23 * 10 + (int)num24;
                    if (source[30] != (byte)58)
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num25 = (uint)source[31] - 48U;
                    uint num26 = (uint)source[32] - 48U;
                    if (num25 > 9U || num26 > 9U)
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int offsetMinutes = (int)num25 * 10 + (int)num26;
                    if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, fraction, num22 == (byte)45, offsetHours, offsetMinutes, out value))
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    bytesConsumed = 33;
                    kind = DateTimeKind.Local;
                    return true;
                default:
                    if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year, month, day, hour, minute, second, fraction, out value))
                    {
                        value = new DateTimeOffset();
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    bytesConsumed = 27;
                    kind = DateTimeKind.Unspecified;
                    return true;
            }
        }

        private static bool TryParseDateTimeOffsetR(ReadOnlySpan<byte> source, uint caseFlipXorMask, out DateTimeOffset dateTimeOffset, out int bytesConsumed)
        {
            if (source.Length < 29)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            DayOfWeek dayOfWeek;
            switch ((uint)((int)((uint)source[0] ^ caseFlipXorMask) << 24 | (int)source[1] << 16 | (int)source[2] << 8) | (uint)source[3])
            {
                case 1181903148:
                    dayOfWeek = DayOfWeek.Friday;
                    break;
                case 1299148332:
                    dayOfWeek = DayOfWeek.Monday;
                    break;
                case 1398895660:
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
                case 1400204844:
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
                case 1416131884:
                    dayOfWeek = DayOfWeek.Thursday;
                    break;
                case 1416979756:
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;
                case 1466262572:
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;
                default:
                    bytesConsumed = 0;
                    dateTimeOffset = new DateTimeOffset();
                    return false;
            }
            if (source[4] != (byte)32)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            uint num1 = (uint)source[5] - 48U;
            uint num2 = (uint)source[6] - 48U;
            if (num1 > 9U || num2 > 9U)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int day = (int)num1 * 10 + (int)num2;
            if (source[7] != (byte)32)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int month;
            switch ((uint)((int)((uint)source[8] ^ caseFlipXorMask) << 24 | (int)source[9] << 16 | (int)source[10] << 8) | (uint)source[11])
            {
                case 1097888288:
                    month = 4;
                    break;
                case 1098213152:
                    month = 8;
                    break;
                case 1147495200:
                    month = 12;
                    break;
                case 1181049376:
                    month = 2;
                    break;
                case 1247899168:
                    month = 1;
                    break;
                case 1249209376:
                    month = 7;
                    break;
                case 1249209888:
                    month = 6;
                    break;
                case 1298231840:
                    month = 3;
                    break;
                case 1298233632:
                    month = 5;
                    break;
                case 1315927584:
                    month = 11;
                    break;
                case 1331917856:
                    month = 10;
                    break;
                case 1399156768:
                    month = 9;
                    break;
                default:
                    bytesConsumed = 0;
                    dateTimeOffset = new DateTimeOffset();
                    return false;
            }
            uint num3 = (uint)source[12] - 48U;
            uint num4 = (uint)source[13] - 48U;
            uint num5 = (uint)source[14] - 48U;
            uint num6 = (uint)source[15] - 48U;
            if (num3 > 9U || num4 > 9U || (num5 > 9U || num6 > 9U))
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int year = (int)num3 * 1000 + (int)num4 * 100 + (int)num5 * 10 + (int)num6;
            if (source[16] != (byte)32)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            uint num7 = (uint)source[17] - 48U;
            uint num8 = (uint)source[18] - 48U;
            if (num7 > 9U || num8 > 9U)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int hour = (int)num7 * 10 + (int)num8;
            if (source[19] != (byte)58)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            uint num9 = (uint)source[20] - 48U;
            uint num10 = (uint)source[21] - 48U;
            if (num9 > 9U || num10 > 9U)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int minute = (int)num9 * 10 + (int)num10;
            if (source[22] != (byte)58)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            uint num11 = (uint)source[23] - 48U;
            uint num12 = (uint)source[24] - 48U;
            if (num11 > 9U || num12 > 9U)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            int second = (int)num11 * 10 + (int)num12;
            if (((uint)((int)source[25] << 24 | (int)((uint)source[26] ^ caseFlipXorMask) << 16 | (int)((uint)source[27] ^ caseFlipXorMask) << 8) | (uint)source[28] ^ caseFlipXorMask) != 541543764U)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, 0, false, 0, 0, out dateTimeOffset))
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            if (dayOfWeek != dateTimeOffset.DayOfWeek)
            {
                bytesConsumed = 0;
                dateTimeOffset = new DateTimeOffset();
                return false;
            }
            bytesConsumed = 29;
            return true;
        }

        /// <summary>
        /// Parses a Decimal at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static unsafe bool TryParse(ReadOnlySpan<byte> source, out decimal value, out int bytesConsumed, char standardFormat = default)
        {
            ParseNumberOptions options;
            switch (standardFormat)
            {
                case char.MinValue:
                case 'E':
                case 'G':
                case 'e':
                case 'g':
                    options = ParseNumberOptions.AllowExponent;
                    break;
                case 'F':
                case 'f':
                    options = (ParseNumberOptions)0;
                    break;
                default:
                    return ThrowHelper.TryParseThrowFormatException(out value, out bytesConsumed);
            }
            NumberBuffer number = new NumberBuffer();
            if (!TryParseNumber(source, ref number, out bytesConsumed, options, out bool textUsedExponentNotation))
            {
                value = default;
                return false;
            }
            if (!textUsedExponentNotation && (standardFormat == 'E' || standardFormat == 'e'))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (number.Digits[0] == (byte)0 && number.Scale == 0)
                number.IsNegative = false;
            value = default;
            if (Number.NumberBufferToDecimal(ref number, ref value))
                return true;
            value = default;
            bytesConsumed = 0;
            return false;
        }

        /// <summary>
        /// Parses a Single at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static unsafe bool TryParse(ReadOnlySpan<byte> source, out float value, out int bytesConsumed, char standardFormat = default)
        {
            if (!TryParseNormalAsFloatingPoint(source, out double num, out bytesConsumed, standardFormat))
                return TryParseAsSpecialFloatingPoint(source, float.PositiveInfinity, float.NegativeInfinity, float.NaN, out value, out bytesConsumed);
            value = (float)num;
            if (!float.IsInfinity(value))
                return true;
            value = 0.0f;
            bytesConsumed = 0;
            return false;
        }

        /// <summary>
        /// Parses a Double at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static unsafe bool TryParse(ReadOnlySpan<byte> source, out double value, out int bytesConsumed, char standardFormat = default)
        {
            return TryParseNormalAsFloatingPoint(source, out value, out bytesConsumed, standardFormat) || TryParseAsSpecialFloatingPoint<double>(source, double.PositiveInfinity, double.NegativeInfinity, double.NaN, out value, out bytesConsumed);
        }

        //
        // Attempt to parse the regular floating points (the ones without names like "Infinity" and "NaN")
        //
        private static bool TryParseNormalAsFloatingPoint(ReadOnlySpan<byte> source, out double value, out int bytesConsumed, char standardFormat)
        {
            ParseNumberOptions options;
            switch (standardFormat)
            {
                case char.MinValue:
                case 'E':
                case 'G':
                case 'e':
                case 'g':
                    options = ParseNumberOptions.AllowExponent;
                    break;
                case 'F':
                case 'f':
                    options = (ParseNumberOptions)0;
                    break;
                default:
                    return ThrowHelper.TryParseThrowFormatException(out value, out bytesConsumed);
            }
            NumberBuffer number = new NumberBuffer();
            if (!TryParseNumber(source, ref number, out bytesConsumed, options, out bool textUsedExponentNotation))
            {
                value = 0.0;
                return false;
            }
            if (!textUsedExponentNotation && (standardFormat == 'E' || standardFormat == 'e'))
            {
                value = 0.0;
                bytesConsumed = 0;
                return false;
            }
            if (number.Digits[0] == (byte)0)
                number.IsNegative = false;
            if (Number.NumberBufferToDouble(ref number, out value))
                return true;
            value = 0.0;
            bytesConsumed = 0;
            return false;
        }

        //
        // Assuming the text doesn't look like a normal floating point, we attempt to parse it as one the special floating point values.
        //
        private static bool TryParseAsSpecialFloatingPoint<T>(ReadOnlySpan<byte> source, T positiveInfinity, T negativeInfinity, T nan, out T value, out int bytesConsumed) where T : struct
        {
            if (source.Length >= 8 && source[0] == (byte)73 && (source[1] == (byte)110 && source[2] == (byte)102) && (source[3] == (byte)105 && source[4] == (byte)110 && (source[5] == (byte)105 && source[6] == (byte)116)) && source[7] == (byte)121)
            {
                value = positiveInfinity;
                bytesConsumed = 8;
                return true;
            }
            if (source.Length >= 9 && source[0] == (byte)45 && (source[1] == (byte)73 && source[2] == (byte)110) && (source[3] == (byte)102 && source[4] == (byte)105 && (source[5] == (byte)110 && source[6] == (byte)105)) && (source[7] == (byte)116 && source[8] == (byte)121))
            {
                value = negativeInfinity;
                bytesConsumed = 9;
                return true;
            }
            if (source.Length >= 3 && source[0] == (byte)78 && (source[1] == (byte)97 && source[2] == (byte)78))
            {
                value = nan;
                bytesConsumed = 3;
                return true;
            }
            value = default;
            bytesConsumed = 0;
            return false;
        }

        /// <summary>
        /// Parses a Guid at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out Guid value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'D':
                    return TryParseGuidCore(source, false, ' ', ' ', out value, out bytesConsumed);
                case 'B':
                    return TryParseGuidCore(source, true, '{', '}', out value, out bytesConsumed);
                case 'N':
                    return TryParseGuidN(source, out value, out bytesConsumed);
                case 'P':
                    return TryParseGuidCore(source, true, '(', ')', out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException(out value, out bytesConsumed);
            }
        }

        // nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn (not very Guid-like, but the format is what it is...)
        private static bool TryParseGuidN(ReadOnlySpan<byte> text, out Guid value, out int bytesConsumed)
        {
            if (text.Length < 32)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            if (!TryParseUInt32X(text.Slice(0, 8), out uint i1, out int justConsumed) || justConsumed != 8)
            {
                value = default;
                bytesConsumed = 0;
                return false; // 8 digits
            }

            if (!TryParseUInt16X(text.Slice(8, 4), out ushort i2, out justConsumed) || justConsumed != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt16X(text.Slice(12, 4), out ushort i3, out justConsumed) || justConsumed != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt16X(text.Slice(16, 4), out ushort i4, out justConsumed) || justConsumed != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            if (!TryParseUInt64X(text.Slice(20), out ulong i5, out justConsumed) || justConsumed != 12)
            {
                value = default;
                bytesConsumed = 0;
                return false; // next 4 digits
            }

            bytesConsumed = 32;
            value = new Guid((int)i1, (short)i2, (short)i3, (byte)(i4 >> 8), (byte)i4,
                (byte)(i5 >> 40), (byte)(i5 >> 32), (byte)(i5 >> 24), (byte)(i5 >> 16), (byte)(i5 >> 8), (byte)i5);
            return true;
        }

        private static bool TryParseGuidCore(ReadOnlySpan<byte> source, bool ends, char begin, char end, out Guid value, out int bytesConsumed)
        {
            int num1 = 36 + (ends ? 2 : 0);
            if (source.Length < num1)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (ends)
            {
                if ((int)source[0] != (int)begin)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }
                source = source.Slice(1);
            }
            uint num2;
            int bytesConsumed1;
            if (!TryParseUInt32X(source, out num2, out bytesConsumed1))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (bytesConsumed1 != 8)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (source[bytesConsumed1] != (byte)45)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            source = source.Slice(9);
            ushort num3;
            if (!TryParseUInt16X(source, out num3, out bytesConsumed1))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (bytesConsumed1 != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (source[bytesConsumed1] != (byte)45)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            source = source.Slice(5);
            ushort num4;
            if (!TryParseUInt16X(source, out num4, out bytesConsumed1))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (bytesConsumed1 != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (source[bytesConsumed1] != (byte)45)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            source = source.Slice(5);
            ushort num5;
            if (!TryParseUInt16X(source, out num5, out bytesConsumed1))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (bytesConsumed1 != 4)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (source[bytesConsumed1] != (byte)45)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            source = source.Slice(5);
            ulong num6;
            if (!TryParseUInt64X(source, out num6, out bytesConsumed1))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (bytesConsumed1 != 12)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            if (ends && (int)source[bytesConsumed1] != (int)end)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }
            bytesConsumed = num1;
            value = new Guid((int)num2, (short)num3, (short)num4, (byte)((uint)num5 >> 8), (byte)num5, (byte)(num6 >> 40), (byte)(num6 >> 32), (byte)(num6 >> 24), (byte)(num6 >> 16), (byte)(num6 >> 8), (byte)num6);
            return true;
        }

        /// <summary>
        /// Parses a SByte at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat == default)
            {
                return TryParseSByteD(source, out value, out bytesConsumed);
            }

            // There's small but measurable overhead when entering the switch block below.
            // We optimize for the default case by hoisting it above the switch block.

            switch (standardFormat)
            {
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseSByteD(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseSByteN(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    value = (sbyte)0;
                    return TryParseByteX(source, out Unsafe.As<sbyte, byte>(ref value), out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<sbyte>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses an Int16 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out short value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat == default)
            {
                return TryParseInt16D(source, out value, out bytesConsumed);
            }

            switch (standardFormat)
            {
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseInt16D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseInt16N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    value = (short)0;
                    return TryParseUInt16X(source, out Unsafe.As<short, ushort>(ref value), out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<short>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses an Int32 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out int value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat == default)
            {
                return TryParseInt32D(source, out value, out bytesConsumed);
            }

            // There's small but measurable overhead when entering the switch block below.
            // We optimize for the default case by hoisting it above the switch block.

            switch (standardFormat)
            {
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseInt32D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseInt32N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    value = 0;
                    return TryParseUInt32X(source, out Unsafe.As<int, uint>(ref value), out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<int>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses an Int64 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out long value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat == default)
            {
                return TryParseInt64D(source, out value, out bytesConsumed);
            }

            // There's small but measurable overhead when entering the switch block below.
            // We optimize for the default case by hoisting it above the switch block.

            switch (standardFormat)
            {
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseInt64D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseInt64N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    value = 0L;
                    return TryParseUInt64X(source, out Unsafe.As<long, ulong>(ref value), out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<long>(out value, out bytesConsumed);
            }
        }

        private static bool TryParseSByteD(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                switch (i1)
                {
                    case 43:
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_16;
                    case 45:
                        num1 = -1;
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_16;
                }
                int num2 = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_17;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_17;
                    }
                    num2 = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num2 = 10 * num2 + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num2 = num2 * 10 + i3 - 48;
                                    if ((long)(uint)num2 > (long)sbyte.MaxValue + (long)((-1 * num1 + 1) / 2) || (uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                        goto label_16;
                                }
                            }
                        }
                    }
                label_17:
                    bytesConsumed = index;
                    value = (sbyte)(num2 * num1);
                    return true;
                }
            }
        label_16:
            bytesConsumed = 0;
            value = (sbyte)0;
            return false;
        }

        private static bool TryParseInt16D(ReadOnlySpan<byte> source, out short value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                switch (i1)
                {
                    case 43:
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_20;
                    case 45:
                        num1 = -1;
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_20;
                }
                int num2 = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_21;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_21;
                    }
                    num2 = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num2 = 10 * num2 + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num2 = 10 * num2 + i3 - 48;
                                    if ((uint)index < (uint)source.Length)
                                    {
                                        int i4 = (int)source[index];
                                        if (ParserHelpers.IsDigit(i4))
                                        {
                                            ++index;
                                            num2 = 10 * num2 + i4 - 48;
                                            if ((uint)index < (uint)source.Length)
                                            {
                                                int i5 = (int)source[index];
                                                if (ParserHelpers.IsDigit(i5))
                                                {
                                                    ++index;
                                                    num2 = num2 * 10 + i5 - 48;
                                                    if ((long)(uint)num2 > (long)short.MaxValue + (long)((-1 * num1 + 1) / 2) || (uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                                        goto label_20;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                label_21:
                    bytesConsumed = index;
                    value = (short)(num2 * num1);
                    return true;
                }
            }
        label_20:
            bytesConsumed = 0;
            value = (short)0;
            return false;
        }

        private static bool TryParseInt32D(ReadOnlySpan<byte> source, out int value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                switch (i1)
                {
                    case 43:
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_31;
                    case 45:
                        num1 = -1;
                        ++index;
                        if ((uint)index < (uint)source.Length)
                        {
                            i1 = (int)source[index];
                            break;
                        }
                        goto label_31;
                }
                int num2 = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_32;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_32;
                    }
                    num2 = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num2 = 10 * num2 + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num2 = 10 * num2 + i3 - 48;
                                    if ((uint)index < (uint)source.Length)
                                    {
                                        int i4 = (int)source[index];
                                        if (ParserHelpers.IsDigit(i4))
                                        {
                                            ++index;
                                            num2 = 10 * num2 + i4 - 48;
                                            if ((uint)index < (uint)source.Length)
                                            {
                                                int i5 = (int)source[index];
                                                if (ParserHelpers.IsDigit(i5))
                                                {
                                                    ++index;
                                                    num2 = 10 * num2 + i5 - 48;
                                                    if ((uint)index < (uint)source.Length)
                                                    {
                                                        int i6 = (int)source[index];
                                                        if (ParserHelpers.IsDigit(i6))
                                                        {
                                                            ++index;
                                                            num2 = 10 * num2 + i6 - 48;
                                                            if ((uint)index < (uint)source.Length)
                                                            {
                                                                int i7 = (int)source[index];
                                                                if (ParserHelpers.IsDigit(i7))
                                                                {
                                                                    ++index;
                                                                    num2 = 10 * num2 + i7 - 48;
                                                                    if ((uint)index < (uint)source.Length)
                                                                    {
                                                                        int i8 = (int)source[index];
                                                                        if (ParserHelpers.IsDigit(i8))
                                                                        {
                                                                            ++index;
                                                                            num2 = 10 * num2 + i8 - 48;
                                                                            if ((uint)index < (uint)source.Length)
                                                                            {
                                                                                int i9 = (int)source[index];
                                                                                if (ParserHelpers.IsDigit(i9))
                                                                                {
                                                                                    ++index;
                                                                                    num2 = 10 * num2 + i9 - 48;
                                                                                    if ((uint)index < (uint)source.Length)
                                                                                    {
                                                                                        int i10 = (int)source[index];
                                                                                        if (ParserHelpers.IsDigit(i10))
                                                                                        {
                                                                                            ++index;
                                                                                            if (num2 <= 214748364)
                                                                                            {
                                                                                                num2 = num2 * 10 + i10 - 48;
                                                                                                if ((long)(uint)num2 > (long)int.MaxValue + (long)((-1 * num1 + 1) / 2) || (uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                                                                                    goto label_31;
                                                                                            }
                                                                                            else
                                                                                                goto label_31;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                label_32:
                    bytesConsumed = index;
                    value = num2 * num1;
                    return true;
                }
            }
        label_31:
            bytesConsumed = 0;
            value = 0;
            return false;
        }

        private static bool TryParseInt64D(ReadOnlySpan<byte> source, out long value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = 0L;
                return false;
            }
            int index1 = 0;
            int num1 = 1;
            if (source[0] == (byte)45)
            {
                index1 = 1;
                num1 = -1;
                if (source.Length <= index1)
                {
                    bytesConsumed = 0;
                    value = 0L;
                    return false;
                }
            }
            else if (source[0] == (byte)43)
            {
                index1 = 1;
                if (source.Length <= index1)
                {
                    bytesConsumed = 0;
                    value = 0L;
                    return false;
                }
            }
            int num2 = 19 + index1;
            long num3 = (long)((int)source[index1] - 48);
            if (num3 < 0L || num3 > 9L)
            {
                bytesConsumed = 0;
                value = 0L;
                return false;
            }
            ulong num4 = (ulong)num3;
            if (source.Length < num2)
            {
                for (int index2 = index1 + 1; index2 < source.Length; ++index2)
                {
                    long num5 = (long)((int)source[index2] - 48);
                    if (num5 < 0L || num5 > 9L)
                    {
                        bytesConsumed = index2;
                        value = (long)num4 * (long)num1;
                        return true;
                    }
                    num4 = (ulong)((long)num4 * 10L + num5);
                }
            }
            else
            {
                for (int index2 = index1 + 1; index2 < num2 - 1; ++index2)
                {
                    long num5 = (long)((int)source[index2] - 48);
                    if (num5 < 0L || num5 > 9L)
                    {
                        bytesConsumed = index2;
                        value = (long)num4 * (long)num1;
                        return true;
                    }
                    num4 = (ulong)((long)num4 * 10L + num5);
                }
                for (int index2 = num2 - 1; index2 < source.Length; ++index2)
                {
                    long num5 = (long)((int)source[index2] - 48);
                    if (num5 < 0L || num5 > 9L)
                    {
                        bytesConsumed = index2;
                        value = (long)num4 * (long)num1;
                        return true;
                    }
                    bool flag1 = num1 > 0;
                    bool flag2 = num5 > 8L || flag1 && num5 > 7L;
                    if (num4 > 922337203685477580UL || num4 == 922337203685477580UL & flag2)
                    {
                        bytesConsumed = 0;
                        value = 0L;
                        return false;
                    }
                    num4 = (ulong)((long)num4 * 10L + num5);
                }
            }
            bytesConsumed = source.Length;
            value = (long)num4 * (long)num1;
            return true;
        }

        private static bool TryParseSByteN(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 45)
                {
                    num1 = -1;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_18;
                }
                else if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_18;
                }
                int num2;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num2 = i1 - 48;
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_15;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            num2 = num2 * 10 + i2 - 48;
                                            continue;
                                        }
                                        goto label_19;
                                }
                            }
                            else
                                goto label_19;
                        }
                        while (num2 <= (int)sbyte.MaxValue + (-1 * num1 + 1) / 2);
                        goto label_18;
                    }
                    else
                        goto label_18;
                }
                else
                {
                    num2 = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_18;
                }
            label_15:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_19;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_18;
                label_19:
                bytesConsumed = index;
                value = (sbyte)(num2 * num1);
                return true;
            }
        label_18:
            bytesConsumed = 0;
            value = (sbyte)0;
            return false;
        }

        private static bool TryParseInt16N(ReadOnlySpan<byte> source, out short value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 45)
                {
                    num1 = -1;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_18;
                }
                else if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_18;
                }
                int num2;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num2 = i1 - 48;
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_15;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            num2 = num2 * 10 + i2 - 48;
                                            continue;
                                        }
                                        goto label_19;
                                }
                            }
                            else
                                goto label_19;
                        }
                        while (num2 <= (int)short.MaxValue + (-1 * num1 + 1) / 2);
                        goto label_18;
                    }
                    else
                        goto label_18;
                }
                else
                {
                    num2 = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_18;
                }
            label_15:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_19;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_18;
                label_19:
                bytesConsumed = index;
                value = (short)(num2 * num1);
                return true;
            }
        label_18:
            bytesConsumed = 0;
            value = (short)0;
            return false;
        }

        private static bool TryParseInt32N(ReadOnlySpan<byte> source, out int value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 45)
                {
                    num1 = -1;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_19;
                }
                else if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_19;
                }
                int num2;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num2 = i1 - 48;
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_16;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            if ((uint)num2 <= 214748364U)
                                            {
                                                num2 = num2 * 10 + i2 - 48;
                                                continue;
                                            }
                                            goto label_19;
                                        }
                                        else
                                            goto label_20;
                                }
                            }
                            else
                                goto label_20;
                        }
                        while ((long)(uint)num2 <= (long)int.MaxValue + (long)((-1 * num1 + 1) / 2));
                        goto label_19;
                    }
                    else
                        goto label_19;
                }
                else
                {
                    num2 = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_19;
                }
            label_16:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_20;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_19;
                label_20:
                bytesConsumed = index;
                value = num2 * num1;
                return true;
            }
        label_19:
            bytesConsumed = 0;
            value = 0;
            return false;
        }

        private static bool TryParseInt64N(ReadOnlySpan<byte> source, out long value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int num1 = 1;
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 45)
                {
                    num1 = -1;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_19;
                }
                else if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_19;
                }
                long num2;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num2 = (long)(i1 - 48);
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_16;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            if ((ulong)num2 <= 922337203685477580UL)
                                            {
                                                num2 = num2 * 10L + (long)i2 - 48L;
                                                continue;
                                            }
                                            goto label_19;
                                        }
                                        else
                                            goto label_20;
                                }
                            }
                            else
                                goto label_20;
                        }
                        while ((ulong)num2 <= (ulong)long.MaxValue + (ulong)((-1 * num1 + 1) / 2));
                        goto label_19;
                    }
                    else
                        goto label_19;
                }
                else
                {
                    num2 = 0L;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_19;
                }
            label_16:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_20;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_19;
                label_20:
                bytesConsumed = index;
                value = num2 * (long)num1;
                return true;
            }
        label_19:
            bytesConsumed = 0;
            value = 0L;
            return false;
        }

        /// <summary>
        /// Parses a Byte at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed, char standardFormat = default)
        {
            if (standardFormat == default)
            {
                return TryParseByteD(source, out value, out bytesConsumed);
            }

            // There's small but measurable overhead when entering the switch block below.
            // We optimize for the default case by hoisting it above the switch block.

            switch (standardFormat)
            {
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseByteD(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseByteN(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    return TryParseByteX(source, out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<byte>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses a UInt16 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseUInt16D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseUInt16N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    return TryParseUInt16X(source, out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<ushort>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses a UInt32 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseUInt32D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseUInt32N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    return TryParseUInt32X(source, out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<uint>(out value, out bytesConsumed);
            }
        }

        /// <summary>
        /// Parses a UInt64 at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
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
        public static bool TryParse(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'D':
                case 'G':
                case 'd':
                case 'g':
                    return TryParseUInt64D(source, out value, out bytesConsumed);
                case 'N':
                case 'n':
                    return TryParseUInt64N(source, out value, out bytesConsumed);
                case 'X':
                case 'x':
                    return TryParseUInt64X(source, out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<ulong>(out value, out bytesConsumed);
            }
        }

        private static bool TryParseByteD(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                int num = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_12;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_12;
                    }
                    num = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num = 10 * num + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num = num * 10 + i3 - 48;
                                    if ((uint)num > (uint)byte.MaxValue || (uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                        goto label_11;
                                }
                            }
                        }
                    }
                label_12:
                    bytesConsumed = index;
                    value = (byte)num;
                    return true;
                }
            }
        label_11:
            bytesConsumed = 0;
            value = (byte)0;
            return false;
        }

        private static bool TryParseUInt16D(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                int num = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_16;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_16;
                    }
                    num = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num = 10 * num + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num = 10 * num + i3 - 48;
                                    if ((uint)index < (uint)source.Length)
                                    {
                                        int i4 = (int)source[index];
                                        if (ParserHelpers.IsDigit(i4))
                                        {
                                            ++index;
                                            num = 10 * num + i4 - 48;
                                            if ((uint)index < (uint)source.Length)
                                            {
                                                int i5 = (int)source[index];
                                                if (ParserHelpers.IsDigit(i5))
                                                {
                                                    ++index;
                                                    num = num * 10 + i5 - 48;
                                                    if ((uint)num > (uint)ushort.MaxValue || (uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                                        goto label_15;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                label_16:
                    bytesConsumed = index;
                    value = (ushort)num;
                    return true;
                }
            }
        label_15:
            bytesConsumed = 0;
            value = (ushort)0;
            return false;
        }

        private static bool TryParseUInt32D(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                int num = 0;
                if (ParserHelpers.IsDigit(i1))
                {
                    if (i1 == 48)
                    {
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                                i1 = (int)source[index];
                            else
                                goto label_27;
                        }
                        while (i1 == 48);
                        if (!ParserHelpers.IsDigit(i1))
                            goto label_27;
                    }
                    num = i1 - 48;
                    ++index;
                    if ((uint)index < (uint)source.Length)
                    {
                        int i2 = (int)source[index];
                        if (ParserHelpers.IsDigit(i2))
                        {
                            ++index;
                            num = 10 * num + i2 - 48;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i3 = (int)source[index];
                                if (ParserHelpers.IsDigit(i3))
                                {
                                    ++index;
                                    num = 10 * num + i3 - 48;
                                    if ((uint)index < (uint)source.Length)
                                    {
                                        int i4 = (int)source[index];
                                        if (ParserHelpers.IsDigit(i4))
                                        {
                                            ++index;
                                            num = 10 * num + i4 - 48;
                                            if ((uint)index < (uint)source.Length)
                                            {
                                                int i5 = (int)source[index];
                                                if (ParserHelpers.IsDigit(i5))
                                                {
                                                    ++index;
                                                    num = 10 * num + i5 - 48;
                                                    if ((uint)index < (uint)source.Length)
                                                    {
                                                        int i6 = (int)source[index];
                                                        if (ParserHelpers.IsDigit(i6))
                                                        {
                                                            ++index;
                                                            num = 10 * num + i6 - 48;
                                                            if ((uint)index < (uint)source.Length)
                                                            {
                                                                int i7 = (int)source[index];
                                                                if (ParserHelpers.IsDigit(i7))
                                                                {
                                                                    ++index;
                                                                    num = 10 * num + i7 - 48;
                                                                    if ((uint)index < (uint)source.Length)
                                                                    {
                                                                        int i8 = (int)source[index];
                                                                        if (ParserHelpers.IsDigit(i8))
                                                                        {
                                                                            ++index;
                                                                            num = 10 * num + i8 - 48;
                                                                            if ((uint)index < (uint)source.Length)
                                                                            {
                                                                                int i9 = (int)source[index];
                                                                                if (ParserHelpers.IsDigit(i9))
                                                                                {
                                                                                    ++index;
                                                                                    num = 10 * num + i9 - 48;
                                                                                    if ((uint)index < (uint)source.Length)
                                                                                    {
                                                                                        int i10 = (int)source[index];
                                                                                        if (ParserHelpers.IsDigit(i10))
                                                                                        {
                                                                                            ++index;
                                                                                            if ((uint)num <= 429496729U && (num != 429496729 || i10 <= 53))
                                                                                            {
                                                                                                num = num * 10 + i10 - 48;
                                                                                                if ((uint)index < (uint)source.Length && ParserHelpers.IsDigit((int)source[index]))
                                                                                                    goto label_26;
                                                                                            }
                                                                                            else
                                                                                                goto label_26;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                label_27:
                    bytesConsumed = index;
                    value = (uint)num;
                    return true;
                }
            }
        label_26:
            bytesConsumed = 0;
            value = 0U;
            return false;
        }

        private static bool TryParseUInt64D(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = 0UL;
                return false;
            }
            ulong num1 = (ulong)((uint)source[0] - 48U);
            if (num1 > 9UL)
            {
                bytesConsumed = 0;
                value = 0UL;
                return false;
            }
            ulong num2 = num1;
            if (source.Length < 19)
            {
                for (int index = 1; index < source.Length; ++index)
                {
                    ulong num3 = (ulong)((uint)source[index] - 48U);
                    if (num3 > 9UL)
                    {
                        bytesConsumed = index;
                        value = num2;
                        return true;
                    }
                    num2 = num2 * 10UL + num3;
                }
            }
            else
            {
                for (int index = 1; index < 18; ++index)
                {
                    ulong num3 = (ulong)((uint)source[index] - 48U);
                    if (num3 > 9UL)
                    {
                        bytesConsumed = index;
                        value = num2;
                        return true;
                    }
                    num2 = num2 * 10UL + num3;
                }
                for (int index = 18; index < source.Length; ++index)
                {
                    ulong num3 = (ulong)((uint)source[index] - 48U);
                    if (num3 > 9UL)
                    {
                        bytesConsumed = index;
                        value = num2;
                        return true;
                    }
                    if (num2 > 1844674407370955161UL || num2 == 1844674407370955161UL && num3 > 5UL)
                    {
                        bytesConsumed = 0;
                        value = 0UL;
                        return false;
                    }
                    num2 = num2 * 10UL + num3;
                }
            }
            bytesConsumed = source.Length;
            value = num2;
            return true;
        }

        private static bool TryParseByteN(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_15;
                }
                int num;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num = i1 - 48;
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_12;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            num = num * 10 + i2 - 48;
                                            continue;
                                        }
                                        goto label_16;
                                }
                            }
                            else
                                goto label_16;
                        }
                        while (num <= (int)byte.MaxValue);
                        goto label_15;
                    }
                    else
                        goto label_15;
                }
                else
                {
                    num = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_15;
                }
            label_12:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_16;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_15;
                label_16:
                bytesConsumed = index;
                value = (byte)num;
                return true;
            }
        label_15:
            bytesConsumed = 0;
            value = (byte)0;
            return false;
        }

        private static bool TryParseUInt16N(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_15;
                }
                int num;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num = i1 - 48;
                        do
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_12;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            num = num * 10 + i2 - 48;
                                            continue;
                                        }
                                        goto label_16;
                                }
                            }
                            else
                                goto label_16;
                        }
                        while (num <= (int)ushort.MaxValue);
                        goto label_15;
                    }
                    else
                        goto label_15;
                }
                else
                {
                    num = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_15;
                }
            label_12:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_16;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_15;
                label_16:
                bytesConsumed = index;
                value = (ushort)num;
                return true;
            }
        label_15:
            bytesConsumed = 0;
            value = (ushort)0;
            return false;
        }

        private static bool TryParseUInt32N(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_16;
                }
                int num;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num = i1 - 48;
                        while (true)
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_13;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            if ((uint)num <= 429496729U && (num != 429496729 || i2 <= 53))
                                            {
                                                num = num * 10 + i2 - 48;
                                                continue;
                                            }
                                            goto label_16;
                                        }
                                        else
                                            goto label_17;
                                }
                            }
                            else
                                goto label_17;
                        }
                    }
                    else
                        goto label_16;
                }
                else
                {
                    num = 0;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_16;
                }
            label_13:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_17;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_16;
                label_17:
                bytesConsumed = index;
                value = (uint)num;
                return true;
            }
        label_16:
            bytesConsumed = 0;
            value = 0U;
            return false;
        }

        private static bool TryParseUInt64N(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
        {
            if (source.Length >= 1)
            {
                int index = 0;
                int i1 = (int)source[index];
                if (i1 == 43)
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i1 = (int)source[index];
                    else
                        goto label_16;
                }
                long num;
                if (i1 != 46)
                {
                    if (ParserHelpers.IsDigit(i1))
                    {
                        num = (long)(i1 - 48);
                        while (true)
                        {
                            ++index;
                            if ((uint)index < (uint)source.Length)
                            {
                                int i2 = (int)source[index];
                                switch (i2)
                                {
                                    case 44:
                                        continue;
                                    case 46:
                                        goto label_13;
                                    default:
                                        if (ParserHelpers.IsDigit(i2))
                                        {
                                            if ((ulong)num <= 1844674407370955161UL && (num != 1844674407370955161L || i2 <= 53))
                                            {
                                                num = num * 10L + (long)i2 - 48L;
                                                continue;
                                            }
                                            goto label_16;
                                        }
                                        else
                                            goto label_17;
                                }
                            }
                            else
                                goto label_17;
                        }
                    }
                    else
                        goto label_16;
                }
                else
                {
                    num = 0L;
                    ++index;
                    if ((uint)index >= (uint)source.Length || source[index] != (byte)48)
                        goto label_16;
                }
            label_13:
                int i3;
                do
                {
                    ++index;
                    if ((uint)index < (uint)source.Length)
                        i3 = (int)source[index];
                    else
                        goto label_17;
                }
                while (i3 == 48);
                if (ParserHelpers.IsDigit(i3))
                    goto label_16;
                label_17:
                bytesConsumed = index;
                value = (ulong)num;
                return true;
            }
        label_16:
            bytesConsumed = 0;
            value = 0UL;
            return false;
        }

        private static bool TryParseByteX(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = (byte)0;
                return false;
            }
            byte[] hexLookup = ParserHelpers.s_hexLookup;
            byte num1 = source[0];
            byte num2 = hexLookup[(int)num1];
            if (num2 == byte.MaxValue)
            {
                bytesConsumed = 0;
                value = (byte)0;
                return false;
            }
            uint num3 = (uint)num2;
            if (source.Length <= 2)
            {
                for (int index = 1; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (byte)num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            else
            {
                for (int index = 1; index < 2; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (byte)num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
                for (int index = 2; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (byte)num3;
                        return true;
                    }
                    if (num3 > 15U)
                    {
                        bytesConsumed = 0;
                        value = (byte)0;
                        return false;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            bytesConsumed = source.Length;
            value = (byte)num3;
            return true;
        }

        private static bool TryParseUInt16X(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = (ushort)0;
                return false;
            }
            byte[] hexLookup = ParserHelpers.s_hexLookup;
            byte num1 = source[0];
            byte num2 = hexLookup[(int)num1];
            if (num2 == byte.MaxValue)
            {
                bytesConsumed = 0;
                value = (ushort)0;
                return false;
            }
            uint num3 = (uint)num2;
            if (source.Length <= 4)
            {
                for (int index = 1; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (ushort)num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            else
            {
                for (int index = 1; index < 4; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (ushort)num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
                for (int index = 4; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = (ushort)num3;
                        return true;
                    }
                    if (num3 > 4095U)
                    {
                        bytesConsumed = 0;
                        value = (ushort)0;
                        return false;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            bytesConsumed = source.Length;
            value = (ushort)num3;
            return true;
        }

        private static bool TryParseUInt32X(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = 0U;
                return false;
            }
            byte[] hexLookup = ParserHelpers.s_hexLookup;
            byte num1 = source[0];
            byte num2 = hexLookup[(int)num1];
            if (num2 == byte.MaxValue)
            {
                bytesConsumed = 0;
                value = 0U;
                return false;
            }
            uint num3 = (uint)num2;
            if (source.Length <= 8)
            {
                for (int index = 1; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            else
            {
                for (int index = 1; index < 8; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
                for (int index = 8; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    if (num3 > 268435455U)
                    {
                        bytesConsumed = 0;
                        value = 0U;
                        return false;
                    }
                    num3 = (num3 << 4) + (uint)num5;
                }
            }
            bytesConsumed = source.Length;
            value = num3;
            return true;
        }

        private static bool TryParseUInt64X(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = 0UL;
                return false;
            }
            byte[] hexLookup = ParserHelpers.s_hexLookup;
            byte num1 = source[0];
            byte num2 = hexLookup[(int)num1];
            if (num2 == byte.MaxValue)
            {
                bytesConsumed = 0;
                value = 0UL;
                return false;
            }
            ulong num3 = (ulong)num2;
            if (source.Length <= 16)
            {
                for (int index = 1; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (ulong)num5;
                }
            }
            else
            {
                for (int index = 1; index < 16; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    num3 = (num3 << 4) + (ulong)num5;
                }
                for (int index = 16; index < source.Length; ++index)
                {
                    byte num4 = source[index];
                    byte num5 = hexLookup[(int)num4];
                    if (num5 == byte.MaxValue)
                    {
                        bytesConsumed = index;
                        value = num3;
                        return true;
                    }
                    if (num3 > 1152921504606846975UL)
                    {
                        bytesConsumed = 0;
                        value = 0UL;
                        return false;
                    }
                    num3 = (num3 << 4) + (ulong)num5;
                }
            }
            bytesConsumed = source.Length;
            value = num3;
            return true;
        }

        private static bool TryParseNumber(ReadOnlySpan<byte> source, ref NumberBuffer number, out int bytesConsumed, ParseNumberOptions options, out bool textUsedExponentNotation)
        {
            Debug.Assert(number.DigitsCount == 0);
            Debug.Assert(number.Scale == 0);
            Debug.Assert(!number.IsNegative);
            Debug.Assert(!number.HasNonZeroTail);

            number.CheckConsistency();
            textUsedExponentNotation = false;

            if (source.Length == 0)
            {
                bytesConsumed = 0;
                return false;
            }

            Span<byte> digits = number.Digits;

            int srcIndex = 0;
            int dstIndex = 0;

            // Consume the leading sign if any.
            byte c = source[srcIndex];
            switch (c)
            {
                case Utf8Constants.Minus:
                    number.IsNegative = true;
                    goto case Utf8Constants.Plus;

                case Utf8Constants.Plus:
                    srcIndex++;
                    if (srcIndex == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    c = source[srcIndex];
                    break;

                default:
                    break;
            }

            int startIndexDigitsBeforeDecimal = srcIndex;
            int digitCount = 0;
            int maxDigitCount = digits.Length - 1;

            // Throw away any leading zeroes
            while (srcIndex != source.Length)
            {
                c = source[srcIndex];
                if (c != '0')
                    break;
                srcIndex++;
            }

            if (srcIndex == source.Length)
            {
                bytesConsumed = srcIndex;
                number.CheckConsistency();
                return true;
            }

            int startIndexNonLeadingDigitsBeforeDecimal = srcIndex;

            int hasNonZeroTail = 0;
            while (srcIndex != source.Length)
            {
                c = source[srcIndex];
                int value = (byte)(c - (byte)('0'));

                if (value > 9)
                {
                    break;
                }

                srcIndex++;
                digitCount++;

                if (digitCount >= maxDigitCount)
                {
                    // For decimal and binary floating-point numbers, we only
                    // need to store digits up to maxDigCount. However, we still
                    // need to keep track of whether any additional digits past
                    // maxDigCount were non-zero, as that can impact rounding
                    // for an input that falls evenly between two representable
                    // results.

                    hasNonZeroTail |= value;
                }
            }
            number.HasNonZeroTail = (hasNonZeroTail != 0);

            int numDigitsBeforeDecimal = srcIndex - startIndexDigitsBeforeDecimal;
            int numNonLeadingDigitsBeforeDecimal = srcIndex - startIndexNonLeadingDigitsBeforeDecimal;

            Debug.Assert(dstIndex == 0);
            int numNonLeadingDigitsBeforeDecimalToCopy = Math.Min(numNonLeadingDigitsBeforeDecimal, maxDigitCount);
            source.Slice(startIndexNonLeadingDigitsBeforeDecimal, numNonLeadingDigitsBeforeDecimalToCopy).CopyTo(digits);
            dstIndex = numNonLeadingDigitsBeforeDecimalToCopy;
            number.Scale = numNonLeadingDigitsBeforeDecimal;

            if (srcIndex == source.Length)
            {
                digits[dstIndex] = 0;
                number.DigitsCount = dstIndex;
                bytesConsumed = srcIndex;
                number.CheckConsistency();
                return true;
            }

            int numDigitsAfterDecimal = 0;
            if (c == Utf8Constants.Period)
            {
                //
                // Parse the digits after the decimal point.
                //

                srcIndex++;
                int startIndexDigitsAfterDecimal = srcIndex;

                while (srcIndex != source.Length)
                {
                    c = source[srcIndex];
                    int value = (byte)(c - (byte)('0'));

                    if (value > 9)
                    {
                        break;
                    }

                    srcIndex++;
                    digitCount++;

                    if (digitCount >= maxDigitCount)
                    {
                        // For decimal and binary floating-point numbers, we only
                        // need to store digits up to maxDigCount. However, we still
                        // need to keep track of whether any additional digits past
                        // maxDigCount were non-zero, as that can impact rounding
                        // for an input that falls evenly between two representable
                        // results.

                        hasNonZeroTail |= value;
                    }
                }
                number.HasNonZeroTail = (hasNonZeroTail != 0);

                numDigitsAfterDecimal = srcIndex - startIndexDigitsAfterDecimal;

                int startIndexOfDigitsAfterDecimalToCopy = startIndexDigitsAfterDecimal;
                if (dstIndex == 0)
                {
                    // Not copied any digits to the Number struct yet. This means we must continue discarding leading zeroes even though they appeared after the decimal point.
                    while (startIndexOfDigitsAfterDecimalToCopy < srcIndex && source[startIndexOfDigitsAfterDecimalToCopy] == '0')
                    {
                        number.Scale--;
                        startIndexOfDigitsAfterDecimalToCopy++;
                    }
                }

                int numDigitsAfterDecimalToCopy = Math.Min(srcIndex - startIndexOfDigitsAfterDecimalToCopy, maxDigitCount - dstIndex);
                source.Slice(startIndexOfDigitsAfterDecimalToCopy, numDigitsAfterDecimalToCopy).CopyTo(digits.Slice(dstIndex));
                dstIndex += numDigitsAfterDecimalToCopy;
                // We "should" really NUL terminate, but there are multiple places we'd have to do this and it is a precondition that the caller pass in a fully zero=initialized Number.

                if (srcIndex == source.Length)
                {
                    if (numDigitsBeforeDecimal == 0 && numDigitsAfterDecimal == 0)
                    {
                        // For compatibility. You can say "5." and ".5" but you can't say "."
                        bytesConsumed = 0;
                        return false;
                    }

                    digits[dstIndex] = 0;
                    number.DigitsCount = dstIndex;
                    bytesConsumed = srcIndex;
                    number.CheckConsistency();
                    return true;
                }
            }

            if (numDigitsBeforeDecimal == 0 && numDigitsAfterDecimal == 0)
            {
                bytesConsumed = 0;
                return false;
            }

            if ((c & ~0x20u) != 'E')
            {
                digits[dstIndex] = 0;
                number.DigitsCount = dstIndex;
                bytesConsumed = srcIndex;
                number.CheckConsistency();
                return true;
            }

            //
            // Parse the exponent after the "E"
            //
            textUsedExponentNotation = true;
            srcIndex++;

            if ((options & ParseNumberOptions.AllowExponent) == 0)
            {
                bytesConsumed = 0;
                return false;
            }

            if (srcIndex == source.Length)
            {
                bytesConsumed = 0;
                return false;
            }

            bool exponentIsNegative = false;
            c = source[srcIndex];
            switch (c)
            {
                case Utf8Constants.Minus:
                    exponentIsNegative = true;
                    goto case Utf8Constants.Plus;

                case Utf8Constants.Plus:
                    srcIndex++;
                    if (srcIndex == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    c = source[srcIndex];
                    break;

                default:
                    break;
            }

            // If the next character isn't a digit, an exponent wasn't specified
            if ((byte)(c - (byte)('0')) > 9)
            {
                bytesConsumed = 0;
                return false;
            }

            if (!TryParseUInt32D(source.Slice(srcIndex), out uint absoluteExponent, out int bytesConsumedByExponent))
            {
                // Since we found at least one digit, we know that any failure to parse means we had an
                // exponent that was larger than uint.MaxValue, and we can just eat characters until the end
                absoluteExponent = uint.MaxValue;

                // This also means that we know there was at least 10 characters and we can "eat" those, and
                // continue eating digits from there
                srcIndex += 10;

                while (srcIndex != source.Length)
                {
                    c = source[srcIndex];
                    int value = (byte)(c - (byte)('0'));

                    if (value > 9)
                    {
                        break;
                    }

                    srcIndex++;
                }
            }

            srcIndex += bytesConsumedByExponent;

            if (exponentIsNegative)
            {
                if (number.Scale < int.MinValue + (long)absoluteExponent)
                {
                    // A scale underflow means all non-zero digits are all so far to the right of the decimal point, no
                    // number format we have will be able to see them. Just pin the scale at the absolute minimum
                    // and let the converter produce a 0 with the max precision available for that type.
                    number.Scale = int.MinValue;
                }
                else
                {
                    number.Scale -= (int)absoluteExponent;
                }
            }
            else
            {
                if (number.Scale > int.MaxValue - (long)absoluteExponent)
                {
                    // A scale overflow means all non-zero digits are all so far to the right of the decimal point, no
                    // number format we have will be able to see them. Just pin the scale at the absolute maximum
                    // and let the converter produce a 0 with the max precision available for that type.
                    number.Scale = int.MaxValue;
                }
                else
                {
                    number.Scale += (int)absoluteExponent;
                }
            }

            digits[dstIndex] = 0;
            number.DigitsCount = dstIndex;
            bytesConsumed = srcIndex;
            number.CheckConsistency();
            return true;
        }

        private static bool TryParseTimeSpanBigG(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
        {
            int start1 = 0;
            byte num1 = 0;
            for (; start1 != source.Length; ++start1)
            {
                num1 = source[start1];
                switch (num1)
                {
                    case 9:
                    case 32:
                        continue;
                    default:
                        goto label_4;
                }
            }
        label_4:
            if (start1 == source.Length)
            {
                value = new TimeSpan();
                bytesConsumed = 0;
                return false;
            }
            bool isNegative = false;
            if (num1 == (byte)45)
            {
                isNegative = true;
                ++start1;
                if (start1 == source.Length)
                {
                    value = new TimeSpan();
                    bytesConsumed = 0;
                    return false;
                }
            }
            uint days;
            int bytesConsumed1;
            if (!TryParseUInt32D(source.Slice(start1), out days, out bytesConsumed1))
            {
                value = new TimeSpan();
                bytesConsumed = 0;
                return false;
            }
            int num2 = start1 + bytesConsumed1;
            if (num2 != source.Length)
            {
                ref ReadOnlySpan<byte> local1 = ref source;
                int index1 = num2;
                int start2 = index1 + 1;
                if (local1[index1] == (byte)58)
                {
                    uint hours;
                    if (!TryParseUInt32D(source.Slice(start2), out hours, out bytesConsumed1))
                    {
                        value = new TimeSpan();
                        bytesConsumed = 0;
                        return false;
                    }
                    int num3 = start2 + bytesConsumed1;
                    if (num3 != source.Length)
                    {
                        ref ReadOnlySpan<byte> local2 = ref source;
                        int index2 = num3;
                        int start3 = index2 + 1;
                        if (local2[index2] == (byte)58)
                        {
                            uint minutes;
                            if (!TryParseUInt32D(source.Slice(start3), out minutes, out bytesConsumed1))
                            {
                                value = new TimeSpan();
                                bytesConsumed = 0;
                                return false;
                            }
                            int num4 = start3 + bytesConsumed1;
                            if (num4 != source.Length)
                            {
                                ref ReadOnlySpan<byte> local3 = ref source;
                                int index3 = num4;
                                int start4 = index3 + 1;
                                if (local3[index3] == (byte)58)
                                {
                                    uint seconds;
                                    if (!TryParseUInt32D(source.Slice(start4), out seconds, out bytesConsumed1))
                                    {
                                        value = new TimeSpan();
                                        bytesConsumed = 0;
                                        return false;
                                    }
                                    int num5 = start4 + bytesConsumed1;
                                    if (num5 != source.Length)
                                    {
                                        ref ReadOnlySpan<byte> local4 = ref source;
                                        int index4 = num5;
                                        int start5 = index4 + 1;
                                        if (local4[index4] == (byte)46)
                                        {
                                            uint fraction;
                                            if (!TryParseTimeSpanFraction(source.Slice(start5), out fraction, out bytesConsumed1))
                                            {
                                                value = new TimeSpan();
                                                bytesConsumed = 0;
                                                return false;
                                            }
                                            int index5 = start5 + bytesConsumed1;
                                            if (!TryCreateTimeSpan(isNegative, days, hours, minutes, seconds, fraction, out value))
                                            {
                                                value = new TimeSpan();
                                                bytesConsumed = 0;
                                                return false;
                                            }
                                            if (index5 != source.Length && (source[index5] == (byte)46 || source[index5] == (byte)58))
                                            {
                                                value = new TimeSpan();
                                                bytesConsumed = 0;
                                                return false;
                                            }
                                            bytesConsumed = index5;
                                            return true;
                                        }
                                    }
                                    value = new TimeSpan();
                                    bytesConsumed = 0;
                                    return false;
                                }
                            }
                            value = new TimeSpan();
                            bytesConsumed = 0;
                            return false;
                        }
                    }
                    value = new TimeSpan();
                    bytesConsumed = 0;
                    return false;
                }
            }
            value = new TimeSpan();
            bytesConsumed = 0;
            return false;
        }

        private static bool TryParseTimeSpanC(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
        {
            TimeSpanSplitter timeSpanSplitter = new TimeSpanSplitter();
            if (!timeSpanSplitter.TrySplitTimeSpan(source, true, out bytesConsumed))
            {
                value = new TimeSpan();
                return false;
            }
            bool isNegative = timeSpanSplitter.IsNegative;
            bool flag;
            switch (timeSpanSplitter.Separators)
            {
                case 0:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, 0U, 0U, 0U, 0U, out value);
                    break;
                case 16777216:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, 0U, 0U, out value);
                    break;
                case 16842752:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0U, out value);
                    break;
                case 16843264:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, out value);
                    break;
                case 33619968:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0U, 0U, out value);
                    break;
                case 33620224:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, 0U, out value);
                    break;
                case 33620226:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, timeSpanSplitter.V5, out value);
                    break;
                default:
                    value = new TimeSpan();
                    flag = false;
                    break;
            }
            if (flag)
                return true;
            bytesConsumed = 0;
            return false;
        }

        /// <summary>
        /// Parses a TimeSpan at the start of a Utf8 string.
        /// </summary>
        /// <param name="source">The Utf8 string to parse</param>
        /// <param name="value">Receives the parsed value</param>
        /// <param name="bytesConsumed">On a successful parse, receives the length in bytes of the substring that was parsed </param>
        /// <param name="standardFormat">Expected format of the Utf8 string</param>
        /// <returns>
        /// true for success. "bytesConsumed" contains the length in bytes of the substring that was parsed.
        /// false if the string was not syntactically valid or an overflow or underflow occurred. "bytesConsumed" is set to 0.
        /// </returns>
        /// <remarks>
        /// Formats supported:
        ///     c/t/T (default) [-][d.]hh:mm:ss[.fffffff]             (constant format)
        ///     G               [-]d:hh:mm:ss.fffffff                 (general long)
        ///     g               [-][d:]h:mm:ss[.f[f[f[f[f[f[f[]]]]]]] (general short)
        /// </remarks>
        /// <exceptions>
        /// <cref>System.FormatException</cref> if the format is not valid for this data type.
        /// </exceptions>
        public static bool TryParse(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed, char standardFormat = default)
        {
            switch (standardFormat)
            {
                case char.MinValue:
                case 'T':
                case 'c':
                case 't':
                    return TryParseTimeSpanC(source, out value, out bytesConsumed);
                case 'G':
                    return TryParseTimeSpanBigG(source, out value, out bytesConsumed);
                case 'g':
                    return TryParseTimeSpanLittleG(source, out value, out bytesConsumed);
                default:
                    return ThrowHelper.TryParseThrowFormatException<TimeSpan>(out value, out bytesConsumed);
            }
        }

        private static bool TryParseTimeSpanFraction(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
        {
            int index1 = 0;
            if (index1 == source.Length)
            {
                value = 0U;
                bytesConsumed = 0;
                return false;
            }
            uint num1 = (uint)source[index1] - 48U;
            if (num1 > 9U)
            {
                value = 0U;
                bytesConsumed = 0;
                return false;
            }
            int index2 = index1 + 1;
            uint num2 = num1;
            int num3 = 1;
            while (index2 != source.Length)
            {
                uint num4 = (uint)source[index2] - 48U;
                if (num4 <= 9U)
                {
                    ++index2;
                    ++num3;
                    if (num3 > 7)
                    {
                        value = 0U;
                        bytesConsumed = 0;
                        return false;
                    }
                    num2 = 10U * num2 + num4;
                }
                else
                    break;
            }
            switch (num3)
            {
                case 2:
                    num2 *= 100000U;
                    goto case 7;
                case 3:
                    num2 *= 10000U;
                    goto case 7;
                case 4:
                    num2 *= 1000U;
                    goto case 7;
                case 5:
                    num2 *= 100U;
                    goto case 7;
                case 6:
                    num2 *= 10U;
                    goto case 7;
                case 7:
                    value = num2;
                    bytesConsumed = index2;
                    return true;
                default:
                    num2 *= 1000000U;
                    goto case 7;
            }
        }

        private static bool TryCreateTimeSpan(bool isNegative, uint days, uint hours, uint minutes, uint seconds, uint fraction, out TimeSpan timeSpan)
        {
            if (hours > 23U || minutes > 59U || seconds > 59U)
            {
                timeSpan = default;
                return false;
            }
            long num1 = ((long)days * 3600L * 24L + (long)hours * 3600L + (long)minutes * 60L + (long)seconds) * 1000L;
            long ticks;
            if (isNegative)
            {
                long num2 = -num1;
                if (num2 < -922337203685477L)
                {
                    timeSpan = default;
                    return false;
                }
                long num3 = num2 * 10000L;
                if (num3 < long.MinValue + (long)fraction)
                {
                    timeSpan = default;
                    return false;
                }
                ticks = num3 - (long)fraction;
            }
            else
            {
                if (num1 > 922337203685477L)
                {
                    timeSpan = default;
                    return false;
                }
                long num2 = num1 * 10000L;
                if (num2 > long.MaxValue - (long)fraction)
                {
                    timeSpan = default;
                    return false;
                }
                ticks = num2 + (long)fraction;
            }
            timeSpan = new TimeSpan(ticks);
            return true;
        }

        private static bool TryParseTimeSpanLittleG(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
        {
            TimeSpanSplitter timeSpanSplitter = new TimeSpanSplitter();
            if (!timeSpanSplitter.TrySplitTimeSpan(source, false, out bytesConsumed))
            {
                value = new TimeSpan();
                return false;
            }
            bool isNegative = timeSpanSplitter.IsNegative;
            bool flag;
            switch (timeSpanSplitter.Separators)
            {
                case 0:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, 0U, 0U, 0U, 0U, out value);
                    break;
                case 16777216:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, 0U, 0U, out value);
                    break;
                case 16842752:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0U, out value);
                    break;
                case 16843008:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, 0U, out value);
                    break;
                case 16843010:
                    flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, timeSpanSplitter.V5, out value);
                    break;
                case 16843264:
                    flag = TryCreateTimeSpan(isNegative, 0U, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, out value);
                    break;
                default:
                    value = new TimeSpan();
                    flag = false;
                    break;
            }
            if (flag)
                return true;
            bytesConsumed = 0;
            return false;
        }

        [Flags]
        private enum ParseNumberOptions
        {
            AllowExponent = 1,
        }

        private enum ComponentParseResult : byte
        {
            NoMoreData,
            Colon,
            Period,
            ParseFailure,
        }

        private struct TimeSpanSplitter
        {
            public uint V1;
            public uint V2;
            public uint V3;
            public uint V4;
            public uint V5;
            public bool IsNegative;
            public uint Separators;

            public bool TrySplitTimeSpan(ReadOnlySpan<byte> source, bool periodUsedToSeparateDay, out int bytesConsumed)
            {
                int i = 0;
                byte b = 0;
                for (; i != source.Length; i++)
                {
                    b = source[i];
                    if (b != 32 && b != 9)
                    {
                        break;
                    }
                }
                if (i == source.Length)
                {
                    bytesConsumed = 0;
                    return false;
                }
                if (b == 45)
                {
                    IsNegative = true;
                    i++;
                    if (i == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                }
                if (!TryParseUInt32D(source.Slice(i), out V1, out var bytesConsumed2))
                {
                    bytesConsumed = 0;
                    return false;
                }
                i += bytesConsumed2;
                ComponentParseResult componentParseResult = ParseComponent(source, periodUsedToSeparateDay, ref i, out V2);
                switch (componentParseResult)
                {
                    case ComponentParseResult.ParseFailure:
                        bytesConsumed = 0;
                        return false;
                    case ComponentParseResult.NoMoreData:
                        bytesConsumed = i;
                        return true;
                    default:
                        Separators |= (uint)componentParseResult << 24;
                        componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V3);
                        switch (componentParseResult)
                        {
                            case ComponentParseResult.ParseFailure:
                                bytesConsumed = 0;
                                return false;
                            case ComponentParseResult.NoMoreData:
                                bytesConsumed = i;
                                return true;
                            default:
                                Separators |= (uint)componentParseResult << 16;
                                componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V4);
                                switch (componentParseResult)
                                {
                                    case ComponentParseResult.ParseFailure:
                                        bytesConsumed = 0;
                                        return false;
                                    case ComponentParseResult.NoMoreData:
                                        bytesConsumed = i;
                                        return true;
                                    default:
                                        Separators |= (uint)componentParseResult << 8;
                                        componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V5);
                                        switch (componentParseResult)
                                        {
                                            case ComponentParseResult.ParseFailure:
                                                bytesConsumed = 0;
                                                return false;
                                            case ComponentParseResult.NoMoreData:
                                                bytesConsumed = i;
                                                return true;
                                            default:
                                                Separators |= (uint)componentParseResult;
                                                if (i != source.Length && (source[i] == 46 || source[i] == 58))
                                                {
                                                    bytesConsumed = 0;
                                                    return false;
                                                }
                                                bytesConsumed = i;
                                                return true;
                                        }
                                }
                        }
                }
            }

            private static ComponentParseResult ParseComponent(ReadOnlySpan<byte> source, bool neverParseAsFraction, ref int srcIndex, out uint value)
            {
                if (srcIndex == source.Length)
                {
                    value = 0u;
                    return ComponentParseResult.NoMoreData;
                }
                byte b = source[srcIndex];
                if (b == 58 || (b == 46 && neverParseAsFraction))
                {
                    srcIndex++;
                    if (!TryParseUInt32D(source.Slice(srcIndex), out value, out var bytesConsumed))
                    {
                        value = 0u;
                        return ComponentParseResult.ParseFailure;
                    }
                    srcIndex += bytesConsumed;
                    if (b != 58)
                    {
                        return ComponentParseResult.Period;
                    }
                    return ComponentParseResult.Colon;
                }
                if (b == 46)
                {
                    srcIndex++;
                    if (!TryParseTimeSpanFraction(source.Slice(srcIndex), out value, out var bytesConsumed2))
                    {
                        value = 0u;
                        return ComponentParseResult.ParseFailure;
                    }
                    srcIndex += bytesConsumed2;
                    return ComponentParseResult.Period;
                }
                value = 0u;
                return ComponentParseResult.NoMoreData;
            }
        }
    }
}
