// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Buffers
{
    /// <summary>
    /// Represents a standard formatting string without using an actual String. A StandardFormat consists of a character (such as 'G', 'D' or 'X')
    /// and an optional precision ranging from 0..99, or the special value NoPrecision.
    /// </summary>
    public readonly struct StandardFormat : IEquatable<StandardFormat>
    {
        /// <summary>
        /// Precision values for format that don't use a precision, or for when the precision is to be unspecified.
        /// </summary>
        public const byte NoPrecision = 255;

        /// <summary>
        /// The maximum valid precision value.
        /// </summary>
        public const byte MaxPrecision = 99;


        private readonly byte _format;
        private readonly byte _precision;

        /// <summary>
        /// The character component of the format.
        /// </summary>
        public char Symbol => (char)_format;

        /// <summary>
        /// The precision component of the format. Ranges from 0..9 or the special value NoPrecision.
        /// </summary>
        public byte Precision => _precision;

        /// <summary>
        /// true if Precision is a value other than NoPrecision
        /// </summary>
        public bool HasPrecision => _precision != NoPrecision;

        /// <summary>
        /// true if the StandardFormat == default(StandardFormat)
        /// </summary>
        public bool IsDefault => _format == 0 && _precision == 0;

        /// <summary>
        /// Create a StandardFormat.
        /// </summary>
        /// <param name="symbol">A type-specific formatting character such as 'G', 'D' or 'X'</param>
        /// <param name="precision">An optional precision ranging from 0..9 or the special value NoPrecision (the default)</param>
        public StandardFormat(char symbol, byte precision = NoPrecision)
        {
            if (precision != NoPrecision && precision > MaxPrecision)
                ThrowHelper.ThrowArgumentOutOfRangeException_PrecisionTooLarge();
            if (symbol != (byte)symbol)
                ThrowHelper.ThrowArgumentOutOfRangeException_SymbolDoesNotFit();

            _format = (byte)symbol;
            _precision = precision;
        }

        /// <summary>
        /// Converts a character to a StandardFormat using the NoPrecision precision.
        /// </summary>
        public static implicit operator StandardFormat(char symbol) => new StandardFormat(symbol);

        /// <summary>
        /// Converts a <see cref="ReadOnlySpan{Char}"/> into a StandardFormat
        /// </summary>
        public static StandardFormat Parse(ReadOnlySpan<char> format)
        {
            ParseHelper(format, out StandardFormat standardFormat, throws: true);

            return standardFormat;
        }

        /// <summary>
        /// Converts a classic .NET format string into a StandardFormat
        /// </summary>
        public static StandardFormat Parse(string format) => format == null ? default : Parse(format.AsSpan());

        private static bool ParseHelper(ReadOnlySpan<char> format, out StandardFormat standardFormat, bool throws = false)
        {
            standardFormat = default;

            if (format.Length == 0)
                return true;

            char symbol = format[0];
            byte precision;
            if (format.Length == 1)
            {
                precision = NoPrecision;
            }
            else
            {
                uint parsedPrecision = 0;
                for (int srcIndex = 1; srcIndex < format.Length; srcIndex++)
                {
                    uint digit = format[srcIndex] - 48u; // '0'
                    if (digit > 9)
                    {
                        return throws ? throw new FormatException(SR2.Format(SR.Argument_CannotParsePrecision, MaxPrecision)) : false;
                    }
                    parsedPrecision = parsedPrecision * 10 + digit;
                    if (parsedPrecision > MaxPrecision)
                    {
                        return throws ? throw new FormatException(SR2.Format(SR.Argument_PrecisionTooLarge, MaxPrecision)) : false;
                    }
                }

                precision = (byte)parsedPrecision;
            }

            standardFormat = new StandardFormat(symbol, precision);
            return true;
        }

        /// <summary>
        /// Returns true if both the Symbol and Precision are equal.
        /// </summary>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is StandardFormat other && Equals(other);

        /// <summary>
        /// Compute a hash code.
        /// </summary>
        public override int GetHashCode() => _format.GetHashCode() ^ _precision.GetHashCode();

        /// <summary>
        /// Returns true if both the Symbol and Precision are equal.
        /// </summary>
        public bool Equals(StandardFormat other) => _format == other._format && _precision == other._precision;

        /// <summary>
        /// Returns the format in classic .NET format.
        /// </summary>
        public unsafe override string ToString()
        {
            char* ptr = stackalloc char[4];
            int length = 0;
            char symbol = Symbol;
            if (symbol != 0)
            {
                ptr[length++] = symbol;
                byte b = Precision;
                if (b != byte.MaxValue)
                {
                    if (b >= 100)
                    {
                        ptr[length++] = (char)(48 + b / 100 % 10);
                        b %= 100;
                    }

                    if (b >= 10)
                    {
                        ptr[length++] = (char)(48 + b / 10 % 10);
                        b %= 10;
                    }

                    ptr[length++] = (char)(48 + b);
                }
            }

            return new string(ptr, 0, length);
        }

        /// <summary>
        /// Returns true if both the Symbol and Precision are equal.
        /// </summary>
        public static bool operator ==(StandardFormat left, StandardFormat right) => left.Equals(right);

        /// <summary>
        /// Returns false if both the Symbol and Precision are equal.
        /// </summary>
        public static bool operator !=(StandardFormat left, StandardFormat right) => !left.Equals(right);
    }
}
