// Decompiled with JetBrains decompiler
// Type: System.Span`1
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable 0809  //warning CS0809: Obsolete member 'Span<T>.Equals(object)' overrides non-obsolete member 'object.Equals(object)'

namespace System
{
    /// <summary>
    /// Span represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type- and memory-safe.
    /// </summary>
    [DebuggerTypeProxy(typeof(SpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly ref struct Span<T>
    {
        private readonly Pinnable<T>? _pinnable;
        private readonly IntPtr _byteOffset;
        /// <summary>The number of elements this Span contains.</summary>
        private readonly int _length;

        /// <summary>
        /// Creates a new span over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(T[]? array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }
            else
            {
                if (default(T) == null && array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();

                _length = array.Length;
                _pinnable = Unsafe.As<Pinnable<T>>(array);
                _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment;
            }
        }

        /// <summary>
        /// Creates a new span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the span.</param>
        /// <param name="length">The number of items in the span.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(T[]? array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                this = default;
                return; // returns default
            }
            else
            {
                if (default(T) == null && array.GetType() != typeof(T[]))
                    ThrowHelper.ThrowArrayTypeMismatchException();
                if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

                _length = length;
                _pinnable = Unsafe.As<Pinnable<T>>(array);
                _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
            }
        }

        /// <summary>
        /// Creates a new span over the target unmanaged buffer.  Clearly this
        /// is quite dangerous, because we are creating arbitrarily typed T's
        /// out of a void*-typed block of memory.  And the length is not checked.
        /// But if this creation is correct, then all subsequent uses are correct.
        /// </summary>
        /// <param name="pointer">An unmanaged pointer to memory.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> is reference type or contains pointers and hence cannot be stored in unmanaged memory.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="length"/> is negative.
        /// </exception>
        [CLSCompliant(false)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(void* pointer, int length)
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);

            _length = length;
            _pinnable = null;
            _byteOffset = new IntPtr(pointer);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span(Pinnable<T>? pinnable, IntPtr byteOffset, int length)
        {
            _length = length;
            _pinnable = pinnable;
            _byteOffset = byteOffset;
        }

        /// <summary>
        /// Returns a reference to specified element of the Span.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public unsafe ref T this[int index]
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_length)
                    ThrowHelper.ThrowIndexOutOfRangeException();
                return ref (_pinnable == null ? ref Unsafe.Add(ref Unsafe.AsRef<T>(_byteOffset.ToPointer()), index) : ref Unsafe.Add(ref Unsafe.AddByteOffset(ref _pinnable.Data!, _byteOffset), index));
            }
        }

        /// <summary>
        /// The number of items in the span.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty => 0 >= (uint)_length; // Workaround for https://github.com/dotnet/runtime/issues/10950

        /// <summary>
        /// Returns false if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator !=(Span<T> left, Span<T> right) => !(left == right);

        /// <summary>
        /// This method is not supported as spans cannot be boxed. To compare two spans, use operator==.
        /// <exception cref="System.NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        /// </summary>
        [Obsolete("Equals() on Span will always throw an exception. Use == instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            throw new NotSupportedException(SR.NotSupported_CannotCallEqualsOnSpan);
        }

        /// <summary>
        /// This method is not supported as spans cannot be boxed.
        /// <exception cref="System.NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        /// </summary>
        [Obsolete("GetHashCode() on Span will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            throw new NotSupportedException(SR.NotSupported_CannotCallGetHashCodeOnSpan);
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Span{T}"/>
        /// </summary>
        public static implicit operator Span<T>(T[]? array) => new Span<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="Span{T}"/>
        /// </summary>
        public static implicit operator Span<T>(ArraySegment<T> segment) =>
            new Span<T>(segment.Array, segment.Offset, segment.Count);

        /// <summary>
        /// Returns an empty <see cref="Span{T}"/>
        /// </summary>
        public static Span<T> Empty => default;

        /// <summary>Gets an enumerator for this span.</summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>Enumerates the elements of a <see cref="Span{T}"/>.</summary>
        public ref struct Enumerator
        {
            /// <summary>The span being enumerated.</summary>
            private readonly Span<T> _span;
            /// <summary>The next index to yield.</summary>
            private int _index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="span">The span to enumerate.</param>
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<T> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the span.</summary>
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                if (index < _span.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public ref T Current
            {
                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _span[_index];
            }
        }

        /// <summary>
        /// Returns a reference to the 0th element of the Span. If the Span is empty, returns null reference.
        /// It can be used for pinning and is required to support the use of span within a fixed statement.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe ref T GetPinnableReference()
        {
            if (_length == 0)
                return ref Unsafe.NullRef<T>();
            return ref (_pinnable == null ? ref Unsafe.AsRef<T>(_byteOffset.ToPointer()) : ref Unsafe.AddByteOffset(ref _pinnable.Data!, _byteOffset));
        }

        /// <summary>
        /// Clears the contents of this span.
        /// </summary>
        public unsafe void Clear()
        {
            int length = this._length;
            if (length == 0)
                return;
            UIntPtr byteLength = (UIntPtr)((ulong)(uint)length * (ulong)Unsafe.SizeOf<T>());
            if ((Unsafe.SizeOf<T>() & sizeof(IntPtr) - 1) != 0)
            {
                if (_pinnable == null)
                    SpanHelpers.ClearLessThanPointerSized((byte*)_byteOffset.ToPointer(), byteLength);
                else
                    SpanHelpers.ClearLessThanPointerSized(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data!, _byteOffset)), byteLength);
            }
            else if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                SpanHelpers.ClearPointerSizedWithReferences(ref Unsafe.As<T, IntPtr>(ref DangerousGetPinnableReference()), (UIntPtr)(ulong)(length * Unsafe.SizeOf<T>() / sizeof(IntPtr)));
            else
                SpanHelpers.ClearPointerSizedWithoutReferences(ref Unsafe.As<T, byte>(ref DangerousGetPinnableReference()), byteLength);
        }

        /// <summary>
        /// Fills the contents of this span with the given value.
        /// </summary>
        public unsafe void Fill(T value)
        {
            int length = _length;
            if (length == 0)
                return;
            if (Unsafe.SizeOf<T>() == 1)
            {
                byte num = Unsafe.As<T, byte>(ref value);
                if (_pinnable == null)
                    Unsafe.InitBlockUnaligned(_byteOffset.ToPointer(), num, (uint)length);
                else
                    Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data!, _byteOffset)), num, (uint)length);
            }
            else
            {
                ref T local = ref DangerousGetPinnableReference();
                int elementOffset;
                for (elementOffset = 0; elementOffset < (length & -8); elementOffset += 8)
                {
                    Unsafe.Add(ref local, elementOffset) = value;
                    Unsafe.Add(ref local, elementOffset + 1) = value;
                    Unsafe.Add(ref local, elementOffset + 2) = value;
                    Unsafe.Add(ref local, elementOffset + 3) = value;
                    Unsafe.Add(ref local, elementOffset + 4) = value;
                    Unsafe.Add(ref local, elementOffset + 5) = value;
                    Unsafe.Add(ref local, elementOffset + 6) = value;
                    Unsafe.Add(ref local, elementOffset + 7) = value;
                }
                if (elementOffset < (length & -4))
                {
                    Unsafe.Add(ref local, elementOffset) = value;
                    Unsafe.Add(ref local, elementOffset + 1) = value;
                    Unsafe.Add(ref local, elementOffset + 2) = value;
                    Unsafe.Add(ref local, elementOffset + 3) = value;
                    elementOffset += 4;
                }
                for (; elementOffset < length; ++elementOffset)
                    Unsafe.Add(ref local, elementOffset) = value;
            }
        }

        /// <summary>
        /// Copies the contents of this span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination Span is shorter than the source Span.
        /// </exception>
        public void CopyTo(Span<T> destination)
        {
            if (TryCopyTo(destination))
                return;
            ThrowHelper.ThrowArgumentException_DestinationTooShort();
        }

        /// <summary>
        /// Copies the contents of this span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="destination">The span to copy items into.</param>
        /// <returns>If the destination span is shorter than the source span, this method
        /// return false and no data is written to the destination.</returns>
        public bool TryCopyTo(Span<T> destination)
        {
            int length1 = _length;
            int length2 = destination._length;
            if (length1 == 0)
                return true;
            if ((uint)length1 > (uint)length2)
                return false;
            ref T local = ref DangerousGetPinnableReference();
            SpanHelpers.CopyTo(ref destination.DangerousGetPinnableReference(), length2, ref local, length1);
            return true;
        }

        /// <summary>
        /// Returns true if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        public static bool operator ==(Span<T> left, Span<T> right)
        {
            return left._length == right._length && Unsafe.AreSame<T>(ref left.DangerousGetPinnableReference(), ref right.DangerousGetPinnableReference());
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Span{T}"/> to a <see cref="ReadOnlySpan{T}"/>
        /// </summary>
        public static implicit operator ReadOnlySpan<T>(Span<T> span)
        {
            return new ReadOnlySpan<T>(span._pinnable, span._byteOffset, span._length);
        }

        public override unsafe string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                fixed (char* chPtr = &Unsafe.As<T, char>(ref DangerousGetPinnableReference()))
                    return new string(chPtr, 0, _length);
            }

            return $"System.Span<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            return new Span<T>(_pinnable, _byteOffset.Add<T>(start), _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            return new Span<T>(_pinnable, _byteOffset.Add<T>(start), length);
        }

        /// <summary>
        /// Copies the contents of this span into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray()
        {
            if (_length == 0)
                return SpanHelpers.PerTypeValues<T>.EmptyArray;
            T[] objArray = new T[_length];
            CopyTo((Span<T>)objArray);
            return objArray;
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<T> Create(T[]? array, int start)
        {
            if (array == null)
            {
                if (start != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                return new Span<T>();
            }
            if (default(T) == null && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();
            if ((uint)start > (uint)array.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
            IntPtr byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
            int length = array.Length - start;
            return new Span<T>(Unsafe.As<Pinnable<T>>(array), byteOffset, length);
        }

        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe ref T DangerousGetPinnableReference()
        {
            return ref (_pinnable == null
                ? ref Unsafe.AsRef<T>(_byteOffset.ToPointer())
                : ref Unsafe.AddByteOffset(ref _pinnable.Data!, _byteOffset));
        }

        internal Pinnable<T>? Pinnable => _pinnable;

        internal IntPtr ByteOffset => _byteOffset;
    }
}
