﻿// Decompiled with JetBrains decompiler
// Type: System.ReadOnlyMemory`1
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Represents a contiguous region of memory, similar to <see cref="ReadOnlySpan{T}"/>.
    /// Unlike <see cref="ReadOnlySpan{T}"/>, it is not a byref-like type.
    /// </summary>
    [DebuggerTypeProxy(typeof(MemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct ReadOnlyMemory<T>
    {
        // NOTE: With the current implementation, Memory<T> and ReadOnlyMemory<T> must have the same layout,
        // as code uses Unsafe.As to cast between them.

        // The highest order bit of _index is used to discern whether _object is a pre-pinned array.
        // (_index < 0) => _object is a pre-pinned array, so Pin() will not allocate a new GCHandle
        //       (else) => Pin() needs to allocate a new GCHandle to pin the object.
        private readonly object? _object;
        private readonly int _index;
        private readonly int _length;
        internal const int RemoveFlagsBitMask = 2147483647;

        /// <summary>
        /// Creates a new memory over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyMemory(T[]? array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }
            else
            {
                _object = array;
                _index = 0;
                _length = array.Length;
            }
        }

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
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyMemory(T[]? array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
            else
            {
                if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                    ThrowHelper.ThrowArgumentOutOfRangeException();

                _object = array;
                _index = start;
                _length = length;
            }
        }

        /// <summary>Creates a new memory over the existing object, start, and length. No validation is performed.</summary>
        /// <param name="obj">The target object.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyMemory(object? obj, int start, int length)
        {
            _object = obj;
            _index = start;
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory<T>(T[]? array) => new ReadOnlyMemory<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory<T>(ArraySegment<T> segment) => new ReadOnlyMemory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns an empty <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static ReadOnlyMemory<T> Empty => default;

        /// <summary>
        /// The number of items in the memory.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// For <see cref="ReadOnlyMemory{Char}"/>, returns a new instance of string that represents the characters pointed to by the memory.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </summary>
        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                return (_object is string str) ? str.Substring(_index, _length) : Span.ToString();
            }
            return $"System.ReadOnlyMemory<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            }

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyMemory<T>(_object, _index + start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new ReadOnlyMemory<T>(_object, _index + start, length);
        }

        /// <summary>
        /// Returns a span from the memory.
        /// </summary>
        public ReadOnlySpan<T> Span
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_object is null)
                    return default;
                if (_index < 0)
                    return (ReadOnlySpan<T>)((MemoryManager<T>)_object).GetSpan().Slice(_index, _length);
                if (typeof(T) == typeof(char) && _object is string str)
                    return new ReadOnlySpan<T>(Unsafe.As<Pinnable<T>>(str), MemoryExtensions.StringAdjustment, str.Length).Slice(_index, _length);
                return new ReadOnlySpan<T>((T[])_object, _index, _length);
            }
        }

        /// <summary>
        /// Copies the contents of the read-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        ///
        /// <param name="destination">The Memory to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination is shorter than the source.
        /// </exception>
        /// </summary>
        public void CopyTo(Memory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of the readonly-only memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        ///
        /// <returns>If the destination is shorter than the source, this method
        /// return false and no data is written to the destination.</returns>
        /// </summary>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(Memory<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// <exception cref="System.ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        /// </summary>
        public unsafe MemoryHandle Pin()
        {
            if (_object is not null)
            {
                if (_index < 0)
                    return ((MemoryManager<T>)_object).Pin(_index);
                if (typeof(T) == typeof(char) && _object is string str)
                {
                    GCHandle handle = GCHandle.Alloc(str, GCHandleType.Pinned);
                    return new MemoryHandle(Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _index), handle);
                }
                if (!(_object is T[] objArray))
                    return default;
                if (_length < 0)
                    return new MemoryHandle(Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference((Span<T>)objArray)), _index));
                GCHandle handle1 = GCHandle.Alloc(objArray, GCHandleType.Pinned);
                return new MemoryHandle(Unsafe.Add<T>((void*)handle1.AddrOfPinnedObject(), _index), handle1);
            }
            return default;
        }

        /// <summary>
        /// Copies the contents from the memory into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray() => Span.ToArray();

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T> readOnlyMemory)
            {
                return Equals(readOnlyMemory);
            }
            else if (obj is Memory<T> memory)
            {
                return Equals(memory);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the memory points to the same array and has the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public bool Equals(ReadOnlyMemory<T> other)
        {
            return
                _object == other._object &&
                _index == other._index &&
                _length == other._length;
        }

        /// <summary>Returns the hash code for this <see cref="ReadOnlyMemory{T}"/></summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            if (this._object == null)
                return 0;
            int hashCode1 = this._object.GetHashCode();
            int num = this._index;
            int hashCode2 = num.GetHashCode();
            num = this._length;
            int hashCode3 = num.GetHashCode();
            return ReadOnlyMemory<T>.CombineHashCodes(hashCode1, hashCode2, hashCode3);
        }

        private static int CombineHashCodes(int left, int right)
        {
            return (left << 5) + left ^ right;
        }

        private static int CombineHashCodes(int h1, int h2, int h3)
        {
            return ReadOnlyMemory<T>.CombineHashCodes(ReadOnlyMemory<T>.CombineHashCodes(h1, h2), h3);
        }

        /// <summary>Gets the state of the memory as individual fields.</summary>
        /// <param name="start">The offset.</param>
        /// <param name="length">The count.</param>
        /// <returns>The object.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object? GetObjectStartLength(out int start, out int length)
        {
            start = _index;
            length = _length;
            return _object;
        }
    }
}
