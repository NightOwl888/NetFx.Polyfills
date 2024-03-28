// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.Number
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    internal static class Number
    {
        private static readonly ulong[] s_rgval64Power10 = new ulong[30]
        {
            11529215046068469760UL,
            14411518807585587200UL,
            18014398509481984000UL,
            11258999068426240000UL,
            14073748835532800000UL,
            17592186044416000000UL,
            10995116277760000000UL,
            13743895347200000000UL,
            17179869184000000000UL,
            10737418240000000000UL,
            13421772800000000000UL,
            16777216000000000000UL,
            10485760000000000000UL,
            13107200000000000000UL,
            16384000000000000000UL,
            14757395258967641293UL,
            11805916207174113035UL,
            9444732965739290428UL,
            15111572745182864686UL,
            12089258196146291749UL,
            9671406556917033399UL,
            15474250491067253438UL,
            12379400392853802751UL,
            9903520314283042201UL,
            15845632502852867522UL,
            12676506002282294018UL,
            10141204801825835215UL,
            16225927682921336344UL,
            12980742146337069075UL,
            10384593717069655260UL
        };
        private static readonly sbyte[] s_rgexp64Power10 = new sbyte[15]
        {
            (sbyte) 4,
            (sbyte) 7,
            (sbyte) 10,
            (sbyte) 14,
            (sbyte) 17,
            (sbyte) 20,
            (sbyte) 24,
            (sbyte) 27,
            (sbyte) 30,
            (sbyte) 34,
            (sbyte) 37,
            (sbyte) 40,
            (sbyte) 44,
            (sbyte) 47,
            (sbyte) 50
        };
        private static readonly ulong[] s_rgval64Power10By16 = new ulong[42]
        {
            10240000000000000000UL,
            11368683772161602974UL,
            12621774483536188886UL,
            14012984643248170708UL,
            15557538194652854266UL,
            17272337110188889248UL,
            9588073174409622172UL,
            10644899600020376798UL,
            11818212630765741798UL,
            13120851772591970216UL,
            14567071740625403792UL,
            16172698447808779622UL,
            17955302187076837696UL,
            9967194951097567532UL,
            11065809325636130658UL,
            12285516299433008778UL,
            13639663065038175358UL,
            15143067982934716296UL,
            16812182738118149112UL,
            9332636185032188787UL,
            10361307573072618722UL,
            16615349947311448416UL,
            14965776766268445891UL,
            13479973333575319909UL,
            12141680576410806707UL,
            10936253623915059637UL,
            9850501549098619819UL,
            17745086042373215136UL,
            15983352577617880260UL,
            14396524142538228461UL,
            12967236152753103031UL,
            11679847981112819795UL,
            10520271803096747049UL,
            9475818434452569218UL,
            17070116948172427008UL,
            15375394465392026135UL,
            13848924157002783096UL,
            12474001934591998882UL,
            11235582092889474480UL,
            10120112665365530972UL,
            18230774251475056952UL,
            16420821625123739930UL
        };
        private static readonly short[] s_rgexp64Power10By16 = new short[21]
        {
            (short) 54,
            (short) 107,
            (short) 160,
            (short) 213,
            (short) 266,
            (short) 319,
            (short) 373,
            (short) 426,
            (short) 479,
            (short) 532,
            (short) 585,
            (short) 638,
            (short) 691,
            (short) 745,
            (short) 798,
            (short) 851,
            (short) 904,
            (short) 957,
            (short) 1010,
            (short) 1064,
            (short) 1117
        };
        internal const int DECIMAL_PRECISION = 29;

        public static void RoundNumber(ref NumberBuffer number, int pos)
        {
            Span<byte> digits = number.Digits;
            int index = 0;
            while (index < pos && digits[index] != (byte)0)
                ++index;
            if (index == pos && digits[index] >= (byte)53)
            {
                while (index > 0 && digits[index - 1] == (byte)57)
                    --index;
                if (index > 0)
                {
                    ++digits[index - 1];
                }
                else
                {
                    ++number.Scale;
                    digits[0] = (byte)49;
                    index = 1;
                }
            }
            else
            {
                while (index > 0 && digits[index - 1] == (byte)48)
                    --index;
            }
            if (index == 0)
            {
                number.Scale = 0;
                number.IsNegative = false;
            }
            digits[index] = (byte)0;
        }

        internal static bool NumberBufferToDouble(ref NumberBuffer number, out double value)
        {
            double d = Number.NumberToDouble(ref number);
            uint num1 = Number.DoubleHelper.Exponent(d);
            ulong num2 = Number.DoubleHelper.Mantissa(d);
            switch (num1)
            {
                case 0:
                    if (num2 == 0UL)
                    {
                        d = 0.0;
                        break;
                    }
                    break;
                case 2047:
                    value = 0.0;
                    return false;
            }
            value = d;
            return true;
        }

        internal const int DecimalPrecision = 29;
        public static unsafe bool NumberBufferToDecimal(ref NumberBuffer number, ref decimal value)
        {
            number.CheckConsistency();

            byte* p = number.UnsafeDigits;
            int e = number.Scale;
            bool sign = number.IsNegative;
            uint c = *p;
            if (c == 0)
            {
                // To avoid risking an app-compat issue with pre 4.5 (where some app was illegally using Reflection to examine the internal scale bits), we'll only force
                // the scale to 0 if the scale was previously positive (previously, such cases were unparsable to a bug.)
                value = new decimal(0, 0, 0, sign, (byte)Clamp(-e, 0, 28));
                return true;
            }

            if (e > DecimalPrecision)
                return false;

            ulong low64 = 0;
            while (e > -28)
            {
                e--;
                low64 *= 10;
                low64 += c - '0';
                c = *++p;
                if (low64 >= ulong.MaxValue / 10)
                    break;
                if (c == 0)
                {
                    while (e > 0)
                    {
                        e--;
                        low64 *= 10;
                        if (low64 >= ulong.MaxValue / 10)
                            break;
                    }
                    break;
                }
            }

            uint high = 0;
            while ((e > 0 || (c != 0 && e > -28)) &&
              (high < uint.MaxValue / 10 || (high == uint.MaxValue / 10 && (low64 < 0x99999999_99999999 || (low64 == 0x99999999_99999999 && c <= '5')))))
            {
                // multiply by 10
                ulong tmpLow = (uint)low64 * 10UL;
                ulong tmp64 = (uint)(low64 >> 32) * 10UL + (tmpLow >> 32);
                low64 = (uint)tmpLow + (tmp64 << 32);
                high = (uint)(tmp64 >> 32) + high * 10;

                if (c != 0)
                {
                    c -= '0';
                    low64 += c;
                    if (low64 < c)
                        high++;
                    c = *++p;
                }
                e--;
            }

            if (c >= '5')
            {
                if ((c == '5') && ((low64 & 1) == 0))
                {
                    c = *++p;

                    bool hasZeroTail = !number.HasNonZeroTail;

                    // We might still have some additional digits, in which case they need
                    // to be considered as part of hasZeroTail. Some examples of this are:
                    //  * 3.0500000000000000000001e-27
                    //  * 3.05000000000000000000001e-27
                    // In these cases, we will have processed 3 and 0, and ended on 5. The
                    // buffer, however, will still contain a number of trailing zeros and
                    // a trailing non-zero number.

                    while ((c != 0) && hasZeroTail)
                    {
                        hasZeroTail &= (c == '0');
                        c = *++p;
                    }

                    // We should either be at the end of the stream or have a non-zero tail
                    Debug.Assert((c == 0) || !hasZeroTail);

                    if (hasZeroTail)
                    {
                        // When the next digit is 5, the number is even, and all following
                        // digits are zero we don't need to round.
                        goto NoRounding;
                    }
                }

                if (++low64 == 0 && ++high == 0)
                {
                    low64 = 0x99999999_9999999A;
                    high = uint.MaxValue / 10;
                    e++;
                }
            }
        NoRounding:

            if (e > 0)
                return false;

            if (e <= -DecimalPrecision)
            {
                // Parsing a large scale zero can give you more precision than fits in the decimal.
                // This should only happen for actual zeros or very small numbers that round to zero.
                value = new decimal(0, 0, 0, sign, DecimalPrecision - 1);
            }
            else
            {
                value = new decimal((int)low64, (int)(low64 >> 32), (int)high, sign, (byte)-e);
            }
            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException(SR2.Format(SR.Argument_MinMaxValue, min, max));
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        public static void DecimalToNumber(decimal value, ref NumberBuffer number)
        {
            ref MutableDecimal local = ref Unsafe.As<decimal, MutableDecimal>(ref value);
            Span<byte> digits1 = number.Digits;
            number.IsNegative = local.IsNegative;
            int num1 = 29;
            while (local.Mid > 0U | local.High > 0U)
            {
                uint num2 = DecimalDecCalc.DecDivMod1E9(ref local);
                for (int index = 0; index < 9; ++index)
                {
                    digits1[--num1] = (byte)(num2 % 10U + 48U);
                    num2 /= 10U;
                }
            }
            for (uint low = local.Low; low != 0U; low /= 10U)
                digits1[--num1] = (byte)(low % 10U + 48U);
            int num3 = 29 - num1;
            number.Scale = num3 - local.Scale;
            Span<byte> digits2 = number.Digits;
            int index1 = 0;
            while (--num3 >= 0)
                digits2[index1++] = digits1[num1++];
            digits2[index1] = (byte)0;
        }

        private static uint DigitsToInt(ReadOnlySpan<byte> digits, int count)
        {
            uint num;
            Utf8Parser.TryParse(digits.Slice(0, count), out num, out int _, 'D');
            return num;
        }

        private static ulong Mul32x32To64(uint a, uint b)
        {
            return (ulong)a * (ulong)b;
        }

        private static ulong Mul64Lossy(ulong a, ulong b, ref int pexp)
        {
            ulong num = Number.Mul32x32To64((uint)(a >> 32), (uint)(b >> 32)) + (Number.Mul32x32To64((uint)(a >> 32), (uint)b) >> 32) + (Number.Mul32x32To64((uint)a, (uint)(b >> 32)) >> 32);
            if (((long)num & long.MinValue) == 0L)
            {
                num <<= 1;
                --pexp;
            }
            return num;
        }

        private static int abs(int value)
        {
            return value < 0 ? -value : value;
        }

        private static unsafe double NumberToDouble(ref NumberBuffer number)
        {
            ReadOnlySpan<byte> digits = (ReadOnlySpan<byte>)number.Digits;
            int index = 0;
            int numDigits = number.NumDigits;
            int val1_1 = numDigits;
            for (; digits[index] == (byte)48; ++index)
                --val1_1;
            if (val1_1 == 0)
                return 0.0;
            int count1 = Math.Min(val1_1, 9);
            int val1_2 = val1_1 - count1;
            ulong a = (ulong)Number.DigitsToInt(digits, count1);
            if (val1_2 > 0)
            {
                int count2 = Math.Min(val1_2, 9);
                val1_2 -= count2;
                uint b = (uint)(Number.s_rgval64Power10[count2 - 1] >> 64 - (int)Number.s_rgexp64Power10[count2 - 1]);
                a = Number.Mul32x32To64((uint)a, b) + (ulong)Number.DigitsToInt(digits.Slice(9), count2);
            }
            int num1 = number.Scale - (numDigits - val1_2);
            int num2 = Number.abs(num1);
            if (num2 >= 352)
            {
                ulong num3 = num1 > 0 ? 9218868437227405312UL : 0UL;
                if (number.IsNegative)
                    num3 |= 9223372036854775808UL;
                return *(double*)&num3;
            }
            int pexp = 64;
            if (((long)a & -4294967296L) == 0L)
            {
                a <<= 32;
                pexp -= 32;
            }
            if (((long)a & -281474976710656L) == 0L)
            {
                a <<= 16;
                pexp -= 16;
            }
            if (((long)a & -72057594037927936L) == 0L)
            {
                a <<= 8;
                pexp -= 8;
            }
            if (((long)a & -1152921504606846976L) == 0L)
            {
                a <<= 4;
                pexp -= 4;
            }
            if (((long)a & -4611686018427387904L) == 0L)
            {
                a <<= 2;
                pexp -= 2;
            }
            if (((long)a & long.MinValue) == 0L)
            {
                a <<= 1;
                --pexp;
            }
            int num4 = num2 & 15;
            if (num4 != 0)
            {
                int num3 = (int)Number.s_rgexp64Power10[num4 - 1];
                pexp += num1 < 0 ? -num3 + 1 : num3;
                ulong b = Number.s_rgval64Power10[num4 + (num1 < 0 ? 15 : 0) - 1];
                a = Number.Mul64Lossy(a, b, ref pexp);
            }
            int num5 = num2 >> 4;
            if (num5 != 0)
            {
                int num3 = (int)Number.s_rgexp64Power10By16[num5 - 1];
                pexp += num1 < 0 ? -num3 + 1 : num3;
                ulong b = Number.s_rgval64Power10By16[num5 + (num1 < 0 ? 21 : 0) - 1];
                a = Number.Mul64Lossy(a, b, ref pexp);
            }
            if (((int)a & 1024) != 0)
            {
                ulong num3 = a + 1023UL + (ulong)((int)a >> 11 & 1);
                if (num3 < a)
                {
                    num3 = num3 >> 1 | 9223372036854775808UL;
                    ++pexp;
                }
                a = num3;
            }
            int num6 = pexp + 1022;
            ulong num7 = num6 > 0 ? (num6 < 2047 ? (ulong)(((long)num6 << 52) + ((long)(a >> 11) & 4503599627370495L)) : 9218868437227405312UL) : (num6 != -52 || a < 9223372036854775896UL ? (num6 > -52 ? a >> -num6 + 11 + 1 : 0UL) : 1UL);
            if (number.IsNegative)
                num7 |= 9223372036854775808UL;
            return *(double*)&num7;
        }

        private static class DoubleHelper
        {
            public static unsafe uint Exponent(double d)
            {
                return *(uint*)((byte*)&d + 4) >> 20 & 2047U;
            }

            public static unsafe ulong Mantissa(double d)
            {
                return (ulong)*(uint*)&d | (ulong)(*(uint*)((byte*)&d + 4) & 1048575U) << 32;
            }

            public static unsafe bool Sign(double d)
            {
                return *(uint*)((byte*)&d + 4) >> 31 > 0U;
            }
        }
    }
}
