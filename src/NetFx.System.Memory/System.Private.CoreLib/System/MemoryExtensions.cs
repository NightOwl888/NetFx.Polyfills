// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.MemoryExtensions
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Extension methods for Span{T}, Memory{T}, and friends.
    /// </summary>
    public static partial class MemoryExtensions
    {
        internal static readonly IntPtr StringAdjustment = MeasureStringAdjustment();

        /// <summary>
        /// Creates a new span over the portion of the target array.
        /// </summary>
        public static Span<T> AsSpan<T>(this T[]? array, int start)
        {
            return Span<T>.Create(array, start);
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string? text)
        {
            return text == null
                ? default
                : new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment, text.Length);
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;text.Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string? text, int start)
        {
            if (text == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }
            if ((uint)start > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, text.Length - start);
        }

        /// <summary>
        /// Creates a new readonly span over the portion of the target string.
        /// </summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index or <paramref name="length"/> is not in range.
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string? text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }
            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        public static ReadOnlyMemory<char> AsMemory(this string? text)
        {
            return text == null
                ? default
                : new ReadOnlyMemory<char>(text, 0, text.Length);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;text.Length).
        /// </exception>
        public static ReadOnlyMemory<char> AsMemory(this string? text, int start)
        {
            if (text == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }
            if ((uint)start > (uint)text.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlyMemory<char>(text, start, text.Length - start);
        }

        /// <summary>Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string.</summary>
        /// <param name="text">The target string.</param>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <remarks>Returns default when <paramref name="text"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index or <paramref name="length"/> is not in range.
        /// </exception>
        public static ReadOnlyMemory<char> AsMemory(this string? text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return default;
            }
            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new ReadOnlyMemory<char>(text, start, length);
        }


        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference<T>(span)), Unsafe.As<T, byte>(ref value), span.Length);

            return typeof(T) == typeof(char)
                ? SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length)
                : SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length)
                : SpanHelpers.IndexOf<T>(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
            return typeof(T) == typeof(char)
                ? SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length)
                : SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length)
                : SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using IEquatable{T}.Equals(T).
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
        {
            int length = span.Length;
            return default(T) != null && 
                IsTypeComparableAsBytes<T>(out NUInt size)
                ? length == other.Length && SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (NUInt)length * size)
                : length == other.Length && SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
        }

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        public static int SequenceCompareTo<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference<T>(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);
            
            return typeof(T) == typeof(char)
                ? SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length)
                : SpanHelpers.SequenceCompareTo<T>(ref MemoryMarshal.GetReference<T>(span), span.Length, ref MemoryMarshal.GetReference<T>(other), other.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);

            return typeof(T) == typeof(char)
                ? SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length)
                : SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its first occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length)
                : SpanHelpers.IndexOf(ref MemoryMarshal.GetReference<T>(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Searches for the specified value and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);

            return typeof(T) == typeof(char)
                ? SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length)
                : SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        /// <summary>
        /// Searches for the specified sequence and returns the index of its last occurrence. If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value">The sequence to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length)
                : SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference<T>(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the first index of any of the specified values similar to calling IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length)
                : SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length)
                : SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length)
                : SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length)
                : SpanHelpers.LastIndexOfAny<T>(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length)
                : SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="value0">One of the values to search for.</param>
        /// <param name="value1">One of the values to search for.</param>
        /// <param name="value2">One of the values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte)
                ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length)
                : SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        /// <summary>
        /// Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="values">The set of values to search for.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            return typeof(T) == typeof(byte) ? SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference<T>(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference<T>(values)), values.Length) : SpanHelpers.LastIndexOfAny<T>(ref MemoryMarshal.GetReference<T>(span), span.Length, ref MemoryMarshal.GetReference<T>(values), values.Length);
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements using IEquatable{T}.Equals(T).
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
        {
            int length = span.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length == other.Length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (NUInt)length * size);
                }

                return false;
            }

            if (length == other.Length)
            {
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
            }

            return false;
        }

        /// <summary>
        /// Determines the relative order of the sequences being compared by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SequenceCompareTo<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
        {
            if (typeof(T) == typeof(byte))
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);

            return typeof(T) == typeof(char)
                ? SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length)
                : SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(other), other.Length);
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = value.Length;
            return default(T) != null && IsTypeComparableAsBytes<T>(out NUInt size)
                ? length <= span.Length && SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length * size)
                : length <= span.Length && SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), length);
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = value.Length;
            return default(T) != null && IsTypeComparableAsBytes<T>(out NUInt size)
                ? length <= span.Length && SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length * size)
                : length <= span.Length && SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference<T>(span), ref MemoryMarshal.GetReference(value), length);
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length1 = span.Length;
            int length2 = value.Length;
            return default(T) != null &&
                IsTypeComparableAsBytes<T>(out NUInt size)
                ? length2 <= length1 && SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length1 - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length2 * size)
                : length2 <= length1 && SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length1 - length2), ref MemoryMarshal.GetReference(value), length2);
        }

        /// <summary>
        /// Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = span.Length;
            int length2 = value.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out NUInt size))
            {
                if (length2 <= length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length2 * size);
                }

                return false;
            }

            if (length2 <= length)
            {
                return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2), ref MemoryMarshal.GetReference(value), length2);
            }

            return false;
        }

        /// <summary>
        /// Reverses the sequence of the elements in the entire span.
        /// </summary>
        public static void Reverse<T>(this Span<T> span)
        {
            ref T local = ref MemoryMarshal.GetReference<T>(span);
            int elementOffset1 = 0;
            for (int elementOffset2 = span.Length - 1; elementOffset1 < elementOffset2; --elementOffset2)
            {
                T obj = Unsafe.Add<T>(ref local, elementOffset1);
                Unsafe.Add<T>(ref local, elementOffset1) = Unsafe.Add<T>(ref local, elementOffset2);
                Unsafe.Add<T>(ref local, elementOffset2) = obj;
                ++elementOffset1;
            }
        }

        /// <summary>
        /// Creates a new span over the target array.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this T[]? array)
        {
            return new Span<T>(array);
        }

        /// <summary>
        /// Creates a new Span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the Span.</param>
        /// <param name="length">The number of items in the Span.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this T[]? array, int start, int length)
        {
            return new Span<T>(array, start, length);
        }

        /// <summary>
        /// Creates a new span over the portion of the target array segment.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment)
        {
            return new Span<T>(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Creates a new Span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the Span.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start)
        {
            if ((uint)start > segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new Span<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        /// <summary>
        /// Creates a new Span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the Span.</param>
        /// <param name="length">The number of items in the Span.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start, int length)
        {
            if ((uint)start > segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            if ((uint)length > (segment.Count - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            return new Span<T>(segment.Array, segment.Offset + start, length);
        }

        /// <summary>
        /// Creates a new memory over the target array.
        /// </summary>
        public static Memory<T> AsMemory<T>(this T[]? array) => new Memory<T>(array);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;array.Length).
        /// </exception>
        public static Memory<T> AsMemory<T>(this T[]? array, int start) => new Memory<T>(array, start);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        public static Memory<T> AsMemory<T>(this T[]? array, int start, int length) => new Memory<T>(array, start, length);

        /// <summary>
        /// Creates a new memory over the portion of the target array.
        /// </summary>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment) => new Memory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start)
        {
            if ((uint)start > segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            return new Memory<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        /// <summary>
        /// Creates a new memory over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="segment">The target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>Returns default when <paramref name="segment"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="segment"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;segment.Count).
        /// </exception>
        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start, int length)
        {
            if ((uint)start > segment.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            if ((uint)length > (segment.Count - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            return new Memory<T>(segment.Array, segment.Offset + start, length);
        }

        /// <summary>
        /// Copies the contents of the array into the span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        ///
        ///<param name="source">The array to copy items from.</param>
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination Span is shorter than the source array.
        /// </exception>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[]? source, Span<T> destination)
        {
            new ReadOnlySpan<T>(source).CopyTo(destination);
        }

        /// <summary>
        /// Copies the contents of the array into the memory. If the source
        /// and destinations overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        ///
        ///<param name="source">The array to copy items from.</param>
        /// <param name="destination">The memory to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination is shorter than the source array.
        /// </exception>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[]? source, Memory<T> destination)
        {
            source.CopyTo(destination.Span);
        }


        //
        //  Overlaps
        //  ========
        //
        //  The following methods can be used to determine if two sequences
        //  overlap in memory.
        //
        //  Two sequences overlap if they have positions in common and neither
        //  is empty. Empty sequences do not overlap with any other sequence.
        //
        //  If two sequences overlap, the element offset is the number of
        //  elements by which the second sequence is offset from the first
        //  sequence (i.e., second minus first). An exception is thrown if the
        //  number is not a whole number, which can happen when a sequence of a
        //  smaller type is cast to a sequence of a larger type with unsafe code
        //  or NonPortableCast. If the sequences do not overlap, the offset is
        //  meaningless and arbitrarily set to zero.
        //
        //  Implementation
        //  --------------
        //
        //  Implementing this correctly is quite tricky due of two problems:
        //
        //  * If the sequences refer to two different objects on the managed
        //    heap, the garbage collector can move them freely around or change
        //    their relative order in memory.
        //
        //  * The distance between two sequences can be greater than
        //    int.MaxValue (on a 32-bit system) or long.MaxValue (on a 64-bit
        //    system).
        //
        //  (For simplicity, the following text assumes a 32-bit system, but
        //  everything also applies to a 64-bit system if every 32 is replaced a
        //  64.)
        //
        //  The first problem is solved by calculating the distance with exactly
        //  one atomic operation. If the garbage collector happens to move the
        //  sequences afterwards and the sequences overlapped before, they will
        //  still overlap after the move and their distance hasn't changed. If
        //  the sequences did not overlap, the distance can change but the
        //  sequences still won't overlap.
        //
        //  The second problem is solved by making all addresses relative to the
        //  start of the first sequence and performing all operations in
        //  unsigned integer arithmetic modulo 2^32.
        //
        //  Example
        //  -------
        //
        //  Let's say there are two sequences, x and y. Let
        //
        //      ref T xRef = MemoryMarshal.GetReference(x)
        //      uint xLength = x.Length * Unsafe.SizeOf<T>()
        //      ref T yRef = MemoryMarshal.GetReference(y)
        //      uint yLength = y.Length * Unsafe.SizeOf<T>()
        //
        //  Visually, the two sequences are located somewhere in the 32-bit
        //  address space as follows:
        //
        //      [----------------------------------------------)                            normal address space
        //      0                                             2^32
        //                            [------------------)                                  first sequence
        //                            xRef            xRef + xLength
        //              [--------------------------)     .                                  second sequence
        //              yRef          .         yRef + yLength
        //              :             .            .     .
        //              :             .            .     .
        //                            .            .     .
        //                            .            .     .
        //                            .            .     .
        //                            [----------------------------------------------)      relative address space
        //                            0            .     .                          2^32
        //                            [------------------)             :                    first sequence
        //                            x1           .     x2            :
        //                            -------------)                   [-------------       second sequence
        //                                         y2                  y1
        //
        //  The idea is to make all addresses relative to xRef: Let x1 be the
        //  start address of x in this relative address space, x2 the end
        //  address of x, y1 the start address of y, and y2 the end address of
        //  y:
        //
        //      nuint x1 = 0
        //      nuint x2 = xLength
        //      nuint y1 = (nuint)Unsafe.ByteOffset(xRef, yRef)
        //      nuint y2 = y1 + yLength
        //
        //  xRef relative to xRef is 0.
        //
        //  x2 is simply x1 + xLength. This cannot overflow.
        //
        //  yRef relative to xRef is (yRef - xRef). If (yRef - xRef) is
        //  negative, casting it to an unsigned 32-bit integer turns it into
        //  (yRef - xRef + 2^32). So, in the example above, y1 moves to the right
        //  of x2.
        //
        //  y2 is simply y1 + yLength. Note that this can overflow, as in the
        //  example above, which must be avoided.
        //
        //  The two sequences do *not* overlap if y is entirely in the space
        //  right of x in the relative address space. (It can't be left of it!)
        //
        //          (y1 >= x2) && (y2 <= 2^32)
        //
        //  Inversely, they do overlap if
        //
        //          (y1 < x2) || (y2 > 2^32)
        //
        //  After substituting x2 and y2 with their respective definition:
        //
        //      == (y1 < xLength) || (y1 + yLength > 2^32)
        //
        //  Since yLength can't be greater than the size of the address space,
        //  the overflow can be avoided as follows:
        //
        //      == (y1 < xLength) || (y1 > 2^32 - yLength)
        //
        //  However, 2^32 cannot be stored in an unsigned 32-bit integer, so one
        //  more change is needed to keep doing everything with unsigned 32-bit
        //  integers:
        //
        //      == (y1 < xLength) || (y1 > -yLength)
        //
        //  Due to modulo arithmetic, this gives exactly same result *except* if
        //  yLength is zero, since 2^32 - 0 is 0 and not 2^32. So the case
        //  y.IsEmpty must be handled separately first.
        //

        /// <summary>
        /// Determines whether two sequences overlap in memory.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other)
        {
            return ((ReadOnlySpan<T>)span).Overlaps(other);
        }

        /// <summary>
        /// Determines whether two sequences overlap in memory and outputs the element offset.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other, out int elementOffset)
        {
            return ((ReadOnlySpan<T>)span).Overlaps(other, out elementOffset);
        }

        /// <summary>
        /// Determines whether two sequences overlap in memory.
        /// </summary>
        public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other)
        {
            if (span.IsEmpty || other.IsEmpty)
                return false;
            IntPtr num = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
            return Unsafe.SizeOf<IntPtr>() == 4
                ? (uint)(int)num < (uint)(span.Length * Unsafe.SizeOf<T>()) || (uint)(int)num > (uint)-(other.Length * Unsafe.SizeOf<T>())
                : (ulong)(long)num < (ulong)span.Length * (ulong)Unsafe.SizeOf<T>() || (ulong)(long)num > (ulong)-((long)other.Length * (long)Unsafe.SizeOf<T>());
        }

        /// <summary>
        /// Determines whether two sequences overlap in memory and outputs the element offset.
        /// </summary>
        public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, out int elementOffset)
        {
            if (span.IsEmpty || other.IsEmpty)
            {
                elementOffset = 0;
                return false;
            }
            IntPtr num = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
            if (Unsafe.SizeOf<IntPtr>() == 4)
            {
                if ((uint)(int)num < (uint)(span.Length * Unsafe.SizeOf<T>()) || (uint)(int)num > (uint)-(other.Length * Unsafe.SizeOf<T>()))
                {
                    if ((int)num % Unsafe.SizeOf<T>() != 0)
                        ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch();
                    elementOffset = (int)num / Unsafe.SizeOf<T>();
                    return true;
                }
                elementOffset = 0;
                return false;
            }
            if ((ulong)(long)num < (ulong)span.Length * (ulong)Unsafe.SizeOf<T>() || (ulong)(long)num > (ulong)-((long)other.Length * (long)Unsafe.SizeOf<T>()))
            {
                if ((long)num % (long)Unsafe.SizeOf<T>() != 0L)
                    ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch();
                elementOffset = (int)((long)num / (long)Unsafe.SizeOf<T>());
                return true;
            }
            elementOffset = 0;
            return false;
        }

        /// <summary>
        /// Searches an entire sorted <see cref="Span{T}"/> for a value
        /// using the specified <see cref="IComparable{T}"/> generic interface.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this Span<T> span, IComparable<T> comparable)
        {
            return span.BinarySearch<T, IComparable<T>>(comparable);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="Span{T}"/> for a value
        /// using the specified <typeparamref name="TComparable"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(this Span<T> span, TComparable comparable) where TComparable : IComparable<T>
        {
            return BinarySearch((ReadOnlySpan<T>)span, comparable);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="Span{T}"/> for the specified <paramref name="value"/>
        /// using the specified <typeparamref name="TComparer"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="Span{T}"/> to search.</param>
        /// <param name="value">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        /// /// <returns>
        /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="Span{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparer" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(this Span<T> span, T value, TComparer comparer) where TComparer : IComparer<T>
        {
            return ((ReadOnlySpan<T>)span).BinarySearch(value, comparer);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for a value
        /// using the specified <see cref="IComparable{T}"/> generic interface.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        /// <param name="comparable">The <see cref="IComparable{T}"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this ReadOnlySpan<T> span, IComparable<T> comparable)
        {
            return span.BinarySearch<T, IComparable<T>>(comparable);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for a value
        /// using the specified <typeparamref name="TComparable"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparable">The specific type of <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        /// <param name="comparable">The <typeparamref name="TComparable"/> to use when comparing.</param>
        /// <returns>
        /// The zero-based index of <paramref name="comparable"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="comparable"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="comparable"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparable" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable)
            where TComparable : IComparable<T>
        {
            return SpanHelpers.BinarySearch(span, comparable);
        }

        /// <summary>
        /// Searches an entire sorted <see cref="ReadOnlySpan{T}"/> for the specified <paramref name="value"/>
        /// using the specified <typeparamref name="TComparer"/> generic type.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <typeparam name="TComparer">The specific type of <see cref="IComparer{T}"/>.</typeparam>
        /// <param name="span">The sorted <see cref="ReadOnlySpan{T}"/> to search.</param>
        /// <param name="value">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The <typeparamref name="TComparer"/> to use when comparing.</param>
        /// /// <returns>
        /// The zero-based index of <paramref name="value"/> in the sorted <paramref name="span"/>,
        /// if <paramref name="value"/> is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than <paramref name="value"/> or, if there is
        /// no larger element, the bitwise complement of <see cref="ReadOnlySpan{T}.Length"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name = "comparer" /> is <see langword="null"/> .
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer)
            where TComparer : IComparer<T>
        {
            if (comparer == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer);

            SpanHelpers.ComparerComparable<T, TComparer> comparable = new SpanHelpers.ComparerComparable<T, TComparer>(value, comparer!);
            return span.BinarySearch(comparable);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTypeComparableAsBytes<T>(out NUInt size)
        {
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
            {
                size = (NUInt)1;
                return true;
            }

            if (typeof(T) == typeof(char) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort))
            {
                size = (NUInt)2;
                return true;
            }

            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
            {
                size = (NUInt)4;
                return true;
            }

            if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
            {
                size = (NUInt)8;
                return true;
            }

            size = default;
            return false;
        }

        private static unsafe IntPtr MeasureStringAdjustment()
        {
            string str = "a";
            fixed (char* chPtr = str)
                return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<char>>(str).Data, ref Unsafe.AsRef<char>(chPtr));
        }
    }
}
