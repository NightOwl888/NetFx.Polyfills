// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.Buffers.Binary.BinaryPrimitives
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Runtime.InteropServices;

namespace System.Buffers.Binary
{
    /// <summary>
    /// 
    /// </summary>
    public static class BinaryPrimitives
    {
        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified
        /// <see cref="sbyte"/> value, which effectively does nothing for a <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The passed-in value, unmodified.</returns>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReverseEndianness(sbyte value)
        {
            return value;
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="short"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReverseEndianness(short value)
        {
            return (short)(((int)value & (int)byte.MaxValue) << 8 | ((int)value & 65280) >> 8);
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReverseEndianness(int value)
        {
            return (int)ReverseEndianness((uint)value);
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="long"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReverseEndianness(long value)
        {
            return (long)ReverseEndianness((ulong)value);
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified
        /// <see cref="byte"/> value, which effectively does nothing for a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The passed-in value, unmodified.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReverseEndianness(byte value)
        {
            return value;
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="ushort"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReverseEndianness(ushort value)
        {
            return (ushort)(((int)value >> 8) + ((int)value << 8));
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="uint"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReverseEndianness(uint value)
        {
            uint num1 = value & 16711935U;
            uint num2 = value & 4278255360U;
            return (uint)(((int)(num1 >> 8) | (int)num1 << 24) + ((int)num2 << 8 | (int)(num2 >> 24)));
        }

        /// <summary>
        /// Reverses a primitive value by performing an endianness swap of the specified <see cref="ulong"/> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReverseEndianness(ulong value)
        {
            return ((ulong)ReverseEndianness((uint)value) << 32) + (ulong)ReverseEndianness((uint)(value >> 32));
        }

        /// <summary>
        /// Reads a <see cref="short"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="short"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="short"/>.</exception>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
        {
            short num = MemoryMarshal.Read<short>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="int"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain an <see cref="int"/>.</exception>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
        {
            int num = MemoryMarshal.Read<int>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="long"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="long"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="long"/>.</exception>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
        {
            long num = MemoryMarshal.Read<long>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="ushort"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="ushort"/>.</exception>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
        {
            ushort num = MemoryMarshal.Read<ushort>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="uint"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="uint"/>.</exception>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
        {
            uint num = MemoryMarshal.Read<uint>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="ulong"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns><see cref="ulong"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="ulong"/>.</exception>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
        {
            ulong num = MemoryMarshal.Read<ulong>(source);
            if (BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="short"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="short"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt16BigEndian(ReadOnlySpan<byte> source, out short value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain an <see cref="int"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt32BigEndian(ReadOnlySpan<byte> source, out int value)
        {
            bool flag = MemoryMarshal.TryRead<int>(source, out value);
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="long"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="long"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt64BigEndian(ReadOnlySpan<byte> source, out long value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
                value = BinaryPrimitives.ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ushort"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt16BigEndian(ReadOnlySpan<byte> source, out ushort value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="uint"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt32BigEndian(ReadOnlySpan<byte> source, out uint value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="ulong"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ulong"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt64BigEndian(ReadOnlySpan<byte> source, out ulong value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="short"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="short"/>.</exception>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16LittleEndian(ReadOnlySpan<byte> source)
        {
            short num = MemoryMarshal.Read<short>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain an <see cref="int"/>.</exception>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32LittleEndian(ReadOnlySpan<byte> source)
        {
            int num = MemoryMarshal.Read<int>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="long"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="long"/>.</exception>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64LittleEndian(ReadOnlySpan<byte> source)
        {
            long num = MemoryMarshal.Read<long>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="ushort"/>.</exception>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> source)
        {
            ushort num = MemoryMarshal.Read<ushort>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="uint"/>.</exception>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
        {
            uint num = MemoryMarshal.Read<uint>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="ulong"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> is too small to contain a <see cref="ulong"/>.</exception>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
        {
            ulong num = MemoryMarshal.Read<ulong>(source);
            if (!BitConverter.IsLittleEndian)
                num = ReverseEndianness(num);
            return num;
        }

        /// <summary>
        /// Reads a <see cref="short"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="short"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt16LittleEndian(ReadOnlySpan<byte> source, out short value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain an <see cref="int"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt32LittleEndian(ReadOnlySpan<byte> source, out int value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="long"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="long"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadInt64LittleEndian(ReadOnlySpan<byte> source, out long value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="ushort"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ushort"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 2 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt16LittleEndian(ReadOnlySpan<byte> source, out ushort value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="uint"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt32LittleEndian(ReadOnlySpan<byte> source, out uint value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Reads a <see cref="ulong"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ulong"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt64LittleEndian(ReadOnlySpan<byte> source, out ulong value)
        {
            bool flag = MemoryMarshal.TryRead(source, out value);
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return flag;
        }

        /// <summary>
        /// Writes a <see cref="short"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="short"/>.</exception>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16BigEndian(Span<byte> destination, short value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain an <see cref="int"/>.</exception>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32BigEndian(Span<byte> destination, int value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="long"/>.</exception>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64BigEndian(Span<byte> destination, long value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="ushort"/>.</exception>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="uint"/>.</exception>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">destination is too small to contain a <see cref="ulong"/>.</exception>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="short"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="short"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt16BigEndian(Span<byte> destination, short value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain an <see cref="int"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt32BigEndian(Span<byte> destination, int value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="long"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64BigEndian(Span<byte> destination, long value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ushort"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt16BigEndian(Span<byte> destination, ushort value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="uint"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt32BigEndian(Span<byte> destination, uint value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> into a span of bytes, as big endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ulong"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt64BigEndian(Span<byte> destination, ulong value)
        {
            if (BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="short"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="short"/>.</exception>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LittleEndian(Span<byte> destination, short value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain an <see cref="int"/>.</exception>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LittleEndian(Span<byte> destination, int value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="long"/>.</exception>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LittleEndian(Span<byte> destination, long value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="ushort"/>.</exception>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LittleEndian(Span<byte> destination, ushort value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="uint"/>.</exception>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LittleEndian(Span<byte> destination, uint value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="ulong"/>.</exception>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            MemoryMarshal.Write(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="short"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="short"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt16LittleEndian(Span<byte> destination, short value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain an <see cref="int"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt32LittleEndian(Span<byte> destination, int value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="long"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64LittleEndian(Span<byte> destination, long value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ushort"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 2 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt16LittleEndian(Span<byte> destination, ushort value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="uint"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt32LittleEndian(Span<byte> destination, uint value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="ulong"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt64LittleEndian(Span<byte> destination, ulong value)
        {
            if (!BitConverter.IsLittleEndian)
                value = ReverseEndianness(value);
            return MemoryMarshal.TryWrite(destination, ref value);
        }
    }
}
