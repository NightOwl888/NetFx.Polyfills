// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Runtime.InteropServices;

namespace System
{
    public static partial class MemoryExtensions
    {
        /// <summary>
        /// Indicates whether the specified span contains only white-space characters.
        /// </summary>
        public static bool IsWhiteSpace(this ReadOnlySpan<char> span)
        {
            for (int index = 0; index < span.Length; ++index)
            {
                if (!char.IsWhiteSpace(span[index]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the specified <paramref name="value"/> occurs within the <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static bool Contains(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            return span.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>
        /// Determines whether this <paramref name="span"/> and the specified <paramref name="other"/> span have the same characters
        /// when compared using the specified <paramref name="comparisonType"/> option.
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// </summary>
        public static bool Equals(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.SequenceEqual(other);
                case StringComparison.OrdinalIgnoreCase:
                    return span.Length == other.Length && MemoryExtensions.EqualsOrdinalIgnoreCase(span, other);
                default:
                    return span.ToString().Equals(other.ToString(), comparisonType);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EqualsOrdinalIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
        {
            return other.Length == 0 || CompareToOrdinalIgnoreCase(span, other) == 0;
        }

        /// <summary>
        /// Compares the specified <paramref name="span"/> and <paramref name="other"/> using the specified <paramref name="comparisonType"/>,
        /// and returns an integer that indicates their relative position in the sort order.
        /// <param name="span">The source span.</param>
        /// <param name="other">The value to compare with the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="other"/> are compared.</param>
        /// </summary>
        public static int CompareTo(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
        {
            return comparisonType switch
            {
                StringComparison.Ordinal => span.SequenceCompareTo(other),
                StringComparison.OrdinalIgnoreCase => CompareToOrdinalIgnoreCase(span, other),
                _ => string.Compare(span.ToString(), other.ToString(), comparisonType),
            };
        }

        private unsafe static int CompareToOrdinalIgnoreCase(ReadOnlySpan<char> strA, ReadOnlySpan<char> strB)
        {
            int num = Math.Min(strA.Length, strB.Length);
            int num2 = num;
            fixed (char* ptr = &MemoryMarshal.GetReference(strA))
            {
                fixed (char* ptr3 = &MemoryMarshal.GetReference(strB))
                {
                    char* ptr2 = ptr;
                    char* ptr4 = ptr3;
                    while (num != 0 && *ptr2 <= '\u007f' && *ptr4 <= '\u007f')
                    {
                        int num3 = *ptr2;
                        int num4 = *ptr4;
                        if (num3 == num4)
                        {
                            ptr2++;
                            ptr4++;
                            num--;
                            continue;
                        }

                        if ((uint)(num3 - 97) <= 25u)
                        {
                            num3 -= 32;
                        }

                        if ((uint)(num4 - 97) <= 25u)
                        {
                            num4 -= 32;
                        }

                        if (num3 != num4)
                        {
                            return num3 - num4;
                        }

                        ptr2++;
                        ptr4++;
                        num--;
                    }

                    if (num == 0)
                    {
                        return strA.Length - strB.Length;
                    }

                    num2 -= num;
                    return string.Compare(strA.Slice(num2).ToString(), strB.Slice(num2).ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
        /// <param name="span">The source span.</param>
        /// <param name="value">The value to seek within the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        /// </summary>
        public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            return comparisonType == StringComparison.Ordinal
                ? span.IndexOf(value)
                : span.ToString().IndexOf(value.ToString(), comparisonType);
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase,
        /// using the casing rules of the specified culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        public static int ToLower(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo? culture)
        {
            if (culture == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.culture);
            if (destination.Length < source.Length)
                return -1;
            string str = source.ToString();
            culture!.TextInfo.ToLower(str).AsSpan().CopyTo(destination);
            return source.Length;
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to lowercase,
        /// using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        public static int ToLowerInvariant(this ReadOnlySpan<char> source, Span<char> destination)
        {
            return source.ToLower(destination, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase,
        /// using the casing rules of the specified culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        public static int ToUpper(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
        {
            if (culture == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.culture);
            if (destination.Length < source.Length)
                return -1;
            string str = source.ToString();
            culture!.TextInfo.ToUpper(str).AsSpan().CopyTo(destination);
            return source.Length;
        }

        /// <summary>
        /// Copies the characters from the source span into the destination, converting each character to uppercase
        /// using the casing rules of the invariant culture.
        /// </summary>
        /// <param name="source">The source span.</param>
        /// <param name="destination">The destination span which contains the transformed characters.</param>
        /// <returns>The number of characters written into the destination span. If the destination is too small, returns -1.</returns>
        /// <exception cref="InvalidOperationException">The source and destination buffers overlap.</exception>
        public static int ToUpperInvariant(this ReadOnlySpan<char> source, Span<char> destination)
        {
            return source.ToUpper(destination, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether the end of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the end of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.EndsWith(value);
                case StringComparison.OrdinalIgnoreCase:
                    return value.Length <= span.Length && EqualsOrdinalIgnoreCase(span.Slice(span.Length - value.Length), value);
                default:
                    return span.ToString().EndsWith(value.ToString(), comparisonType);
            }
        }

        /// <summary>
        /// Determines whether the beginning of the <paramref name="span"/> matches the specified <paramref name="value"/> when compared using the specified <paramref name="comparisonType"/> option.
        /// </summary>
        /// <param name="span">The source span.</param>
        /// <param name="value">The sequence to compare to the beginning of the source span.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
        public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.StartsWith(value);
                case StringComparison.OrdinalIgnoreCase:
                    return value.Length <= span.Length && EqualsOrdinalIgnoreCase(span.Slice(0, value.Length), value);
                default:
                    return span.ToString().StartsWith(value.ToString(), comparisonType);
            }
        }
    }
}
