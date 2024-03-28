// Decompiled with JetBrains decompiler
// Type: System.Memory`1
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Memory represents a contiguous region of arbitrary memory similar to <see cref="Span{T}"/>.
    /// Unlike <see cref="Span{T}"/>, it is not a byref-like type.
    /// </summary>
    [DebuggerTypeProxy(typeof(MemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct Memory<T>
    {
        // NOTE: With the current implementation, Memory<T> and ReadOnlyMemory<T> must have the same layout,
        // as code uses Unsafe.As to cast between them.

        // The highest order bit of _index is used to discern whether _object is a pre-pinned array.
        // (_index < 0) => _object is a pre-pinned array, so Pin() will not allocate a new GCHandle
        //       (else) => Pin() needs to allocate a new GCHandle to pin the object.
        private readonly object? _object;
        private readonly int _index;
        private readonly int _length;

        /// <summary>
        /// Creates a new memory over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[]? array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }
            else
            {
                if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();

                _object = array;
                _index = 0;
                _length = array.Length;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe Memory(T[]? array, int start)
        {
            if (array == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                this = default;
                return; // returns default
            }
            else
            {
                if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
                if ((uint)start > (uint)array.Length)
                    ThrowHelper.ThrowArgumentOutOfRangeException();

                _object = array;
                _index = start;
                _length = array.Length - start;
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
        /// <exception cref="ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Memory(T[]? array, int start, int length)
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
                if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
                if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                    ThrowHelper.ThrowArgumentOutOfRangeException();

                _object = array;
                _index = start;
                _length = length;
            }
        }

        /// <summary>
        /// Creates a new memory from a memory manager that provides specific method implementations beginning
        /// at 0 index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="manager">The memory manager.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="length"/> is negative.
        /// </exception>
        /// <remarks>For internal infrastructure only</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(MemoryManager<T> manager, int length)
        {
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            _object = manager;
            _index = int.MinValue;
            _length = length;
        }

        /// <summary>
        /// Creates a new memory from a memory manager that provides specific method implementations beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="manager">The memory manager.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or <paramref name="length"/> is negative.
        /// </exception>
        /// <remarks>For internal infrastructure only</remarks>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(MemoryManager<T> manager, int start, int length)
        {
            if (length < 0 || start < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            _object = manager;
            _index = start | int.MinValue;
            _length = length;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(object? obj, int start, int length)
        {
            // No validation performed in release builds; caller must provide any necessary validation.

            // 'obj is T[]' below also handles things like int[] <-> uint[] being convertible
            Debug.Assert((obj == null)
                || (typeof(T) == typeof(char) && obj is string)
                || (obj is T[])
                || (obj is MemoryManager<T>));

            _object = obj;
            _index = start;
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Memory{T}"/>
        /// </summary>
        public static implicit operator Memory<T>(T[]? array) => new Memory<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="Memory{T}"/>
        /// </summary>
        public static implicit operator Memory<T>(ArraySegment<T> segment) => new Memory<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Memory{T}"/> to a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory<T>(Memory<T> memory) =>
            Unsafe.As<Memory<T>, ReadOnlyMemory<T>>(ref memory);

        /// <summary>
        /// Returns an empty <see cref="Memory{T}"/>
        /// </summary>
        public static Memory<T> Empty => default;

        /// <summary>
        /// The number of items in the memory.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// For <see cref="Memory{Char}"/>, returns a new instance of string that represents the characters pointed to by the memory.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </summary>
        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                return (_object is string str) ? str.Substring(_index, _length) : Span.ToString();
            }
            return $"System.Memory<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            }

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new Memory<T>(_object, _index + start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given memory, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // It is expected for _index + start to be negative if the memory is already pre-pinned.
            return new Memory<T>(_object, _index + start, length);
        }

        /// <summary>
        /// Returns a span from the memory.
        /// </summary>
        public Span<T> Span
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // This property getter has special support for returning a mutable Span<char> that wraps
                // an immutable String instance. This is obviously a dangerous feature and breaks type safety.
                // However, we need to handle the case where a ReadOnlyMemory<char> was created from a string
                // and then cast to a Memory<T>. Such a cast can only be done with unsafe or marshaling code,
                // in which case that's the dangerous operation performed by the dev, and we're just following
                // suit here to make it work as best as possible.

                if (_index < 0)
                    return ((MemoryManager<T>)_object!).GetSpan().Slice(_index & int.MaxValue, _length);
                if (typeof(T) == typeof(char) && _object is string str)
                    return new Span<T>(Unsafe.As<Pinnable<T>>(str), MemoryExtensions.StringAdjustment, str.Length).Slice(_index, _length);
                return _object != null ? new Span<T>((T[])_object, _index, _length & int.MaxValue) : default;
            }
        }

        /// <summary>
        /// Copies the contents of the memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The Memory to copy items into.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination is shorter than the source.
        /// </exception>
        public void CopyTo(Memory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of the memory into the destination. If the source
        /// and destination overlap, this method behaves as if the original values are in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <returns>If the destination is shorter than the source, this method
        /// return false and no data is written to the destination.</returns>
        /// <param name="destination">The span to copy items into.</param>
        public bool TryCopyTo(Memory<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        public unsafe MemoryHandle Pin()
        {
            if (_index < 0)
                return ((MemoryManager<T>)_object!).Pin(_index & int.MaxValue);
            if (typeof(T) == typeof(char) && _object is string str)
            {
                GCHandle handle2 = GCHandle.Alloc(str, GCHandleType.Pinned);
                return new MemoryHandle(Unsafe.Add<T>((void*)handle2.AddrOfPinnedObject(), _index), handle2);
            }
            if (!(_object is T[] objArray))
                return default;

            if (_length < 0)
            {
                return new MemoryHandle(Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference((Span<T>)objArray)), _index));
            }
            GCHandle handle = GCHandle.Alloc(objArray, GCHandleType.Pinned);
            return new MemoryHandle(Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _index), handle);
        }

        /// <summary>
        /// Copies the contents from the memory into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray() => Span.ToArray();

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// Returns true if the object is Memory or ReadOnlyMemory and if both objects point to the same array and have the same length.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T>)
            {
                return ((ReadOnlyMemory<T>)obj).Equals(this);
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
        public bool Equals(Memory<T> other)
        {
            return _object == other._object && _index == other._index && _length == other._length;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
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
            return Memory<T>.CombineHashCodes(hashCode1, hashCode2, hashCode3);
        }

        private static int CombineHashCodes(int left, int right)
        {
            return (left << 5) + left ^ right;
        }

        private static int CombineHashCodes(int h1, int h2, int h3)
        {
            return Memory<T>.CombineHashCodes(Memory<T>.CombineHashCodes(h1, h2), h3);
        }
    }
}
