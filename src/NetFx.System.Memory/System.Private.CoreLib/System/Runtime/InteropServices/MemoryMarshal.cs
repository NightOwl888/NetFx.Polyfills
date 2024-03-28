// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.Runtime.InteropServices.MemoryMarshal
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Provides a collection of methods for interoperating with <see cref="Memory{T}"/>, <see cref="ReadOnlyMemory{T}"/>,
    /// <see cref="Span{T}"/>, and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class MemoryMarshal
    {
        /// <summary>
        /// Casts a Span of one primitive type <typeparamref name="T"/> to Span of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="System.OverflowException">
        /// Thrown if the Length property of the new Span would exceed int.MaxValue.
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsBytes<T>(Span<T> span) where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));

            int length = checked(span.Length * Unsafe.SizeOf<T>());
            return new Span<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
        }

        /// <summary>
        /// Casts a ReadOnlySpan of one primitive type <typeparamref name="T"/> to ReadOnlySpan of bytes.
        /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> contains pointers.
        /// </exception>
        /// <exception cref="System.OverflowException">
        /// Thrown if the Length property of the new Span would exceed int.MaxValue.
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsBytes<T>(ReadOnlySpan<T> span) where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));

            int length = checked(span.Length * Unsafe.SizeOf<T>());
            return new ReadOnlySpan<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
        }

        /// <summary>Creates a <see cref="Memory{T}"/> from a <see cref="ReadOnlyMemory{T}"/>.</summary>
        /// <param name="memory">The <see cref="ReadOnlyMemory{T}"/>.</param>
        /// <returns>A <see cref="Memory{T}"/> representing the same memory as the <see cref="ReadOnlyMemory{T}"/>, but writable.</returns>
        /// <remarks>
        /// <see cref="AsMemory{T}(ReadOnlyMemory{T})"/> must be used with extreme caution.  <see cref="ReadOnlyMemory{T}"/> is used
        /// to represent immutable data and other memory that is not meant to be written to; <see cref="Memory{T}"/> instances created
        /// by <see cref="AsMemory{T}(ReadOnlyMemory{T})"/> should not be written to.  The method exists to enable variables typed
        /// as <see cref="Memory{T}"/> but only used for reading to store a <see cref="ReadOnlyMemory{T}"/>.
        /// </remarks>
        public static Memory<T> AsMemory<T>(ReadOnlyMemory<T> memory) =>
            Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref memory);

        /// <summary>
        /// Returns a reference to the 0th element of the Span. If the Span is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static unsafe ref T GetReference<T>(Span<T> span)
        {
            return ref (span.Pinnable == null
                ? ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer())
                : ref Unsafe.AddByteOffset(ref span.Pinnable.Data!, span.ByteOffset));
        }

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlySpan. If the ReadOnlySpan is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static unsafe ref T GetReference<T>(ReadOnlySpan<T> span)
        {
            return ref (span.Pinnable == null
                ? ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer())
                : ref Unsafe.AddByteOffset(ref span.Pinnable.Data!, span.ByteOffset));
        }

        /// <summary>
        /// Casts a Span of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));

            int length = checked((int)unchecked(checked((long)span.Length * Unsafe.SizeOf<TFrom>()) / Unsafe.SizeOf<TTo>()));
            return new Span<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
        }

        /// <summary>
        /// Casts a ReadOnlySpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
        /// </summary>
        /// <remarks>
        /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
        /// </remarks>
        /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
        /// </exception>
        public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(ReadOnlySpan<TFrom> span)
            where TFrom : struct
            where TTo : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
            if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));

            int length = checked((int)unchecked(checked(span.Length * Unsafe.SizeOf<TFrom>()) / Unsafe.SizeOf<TTo>()));
            return new ReadOnlySpan<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
        }


        /// <summary>
        /// Get an array segment from the underlying memory.
        /// If unable to get the array segment, return false with a default array segment.
        /// </summary>
        public static bool TryGetArray<T>(ReadOnlyMemory<T> memory, out ArraySegment<T> segment)
        {
            object? objectStartLength = memory.GetObjectStartLength(out int start, out int length);
            if (start < 0)
            {
                ArraySegment<T> segment1;
                if (((MemoryManager<T>)objectStartLength!).TryGetArray(out segment1))
                {
                    segment = new ArraySegment<T>(segment1.Array, segment1.Offset + (start & int.MaxValue), length);
                    return true;
                }
            }
            else if (objectStartLength is T[] array)
            {
                segment = new ArraySegment<T>(array, start, length & int.MaxValue);
                return true;
            }
            if ((length & int.MaxValue) == 0)
            {
                segment = new ArraySegment<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);
                return true;
            }
            segment = default;
            return false;
        }

        /// <summary>
        /// Gets an <see cref="MemoryManager{T}"/> from the underlying read-only memory.
        /// If unable to get the <typeparamref name="TManager"/> type, returns false.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <typeparam name="TManager">The type of <see cref="MemoryManager{T}"/> to try and retrive.</typeparam>
        /// <param name="memory">The memory to get the manager for.</param>
        /// <param name="manager">The returned manager of the <see cref="ReadOnlyMemory{T}"/>.</param>
        /// <returns>A <see cref="bool"/> indicating if it was successful.</returns>
        public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory,[NotNullWhen(true)] out TManager? manager)
            where TManager : MemoryManager<T>
        {
            TManager? localManager; // Use register for null comparison rather than byref
            manager = localManager = memory.GetObjectStartLength(out _, out _) as TManager;
#pragma warning disable 8762 // "Parameter 'manager' may not have a null value when exiting with 'true'."
            return localManager != null;
#pragma warning restore 8762
        }

        /// <summary>
        /// Gets an <see cref="MemoryManager{T}"/> and <paramref name="start" />, <paramref name="length" /> from the underlying read-only memory.
        /// If unable to get the <typeparamref name="TManager"/> type, returns false.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <typeparam name="TManager">The type of <see cref="MemoryManager{T}"/> to try and retrive.</typeparam>
        /// <param name="memory">The memory to get the manager for.</param>
        /// <param name="manager">The returned manager of the <see cref="ReadOnlyMemory{T}"/>.</param>
        /// <param name="start">The offset from the start of the <paramref name="manager" /> that the <paramref name="memory" /> represents.</param>
        /// <param name="length">The length of the <paramref name="manager" /> that the <paramref name="memory" /> represents.</param>
        /// <returns>A <see cref="bool"/> indicating if it was successful.</returns>
        public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, [NotNullWhen(true)] out TManager? manager, out int start, out int length)
          where TManager : MemoryManager<T>
        {
            TManager? objectStartLength;
            manager = objectStartLength = memory.GetObjectStartLength(out start, out length) as TManager;
            start &= int.MaxValue;

            Debug.Assert(length >= 0);

            if (objectStartLength == null)
            {
                start = 0;
                length = 0;
                return false;
            }
#pragma warning disable 8762 // "Parameter 'manager' may not have a null value when exiting with 'true'."
            return true;
#pragma warning restore 8762
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> view of the given <paramref name="memory" /> to allow
        /// the <paramref name="memory" /> to be used in existing APIs that take an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="memory" />.</typeparam>
        /// <param name="memory">The ReadOnlyMemory to view as an <see cref="IEnumerable{T}"/></param>
        /// <returns>An <see cref="IEnumerable{T}"/> view of the given <paramref name="memory" /></returns>
        public static IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory)
        {
            for (int i = 0; i < memory.Length; ++i)
                yield return memory.Span[i];
        }

        /// <summary>Attempts to get the underlying <see cref="string"/> from a <see cref="ReadOnlyMemory{T}"/>.</summary>
        /// <param name="memory">The memory that may be wrapping a <see cref="string"/> object.</param>
        /// <param name="text">The string.</param>
        /// <param name="start">The starting location in <paramref name="text"/>.</param>
        /// <param name="length">The number of items in <paramref name="text"/>.</param>
        /// <returns></returns>
        public static bool TryGetString(ReadOnlyMemory<char> memory, [NotNullWhen(true)] out string? text, out int start, out int length)
        {
            if (memory.GetObjectStartLength(out int offset, out int count) is string s)
            {
                Debug.Assert(offset >= 0);
                Debug.Assert(count >= 0);
                text = s;
                start = offset;
                length = count;
                return true;
            }
            else
            {
                text = null;
                start = 0;
                length = 0;
                return false;
            }
        }

        /// <summary>
        /// Reads a structure of type T out of a read-only span of bytes.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ReadOnlySpan<byte> source)
            where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            }
            if (Unsafe.SizeOf<T>() > source.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            }
            return Unsafe.ReadUnaligned<T>(ref GetReference(source));
        }

        /// <summary>
        /// Reads a structure of type T out of a span of bytes.
        /// <returns>If the span is too small to contain the type T, return false.</returns>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRead<T>(ReadOnlySpan<byte> source, out T value)
            where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            }
            if (Unsafe.SizeOf<T>() > (uint)source.Length)
            {
                value = default;
                return false;
            }
            value = Unsafe.ReadUnaligned<T>(ref GetReference(source));
            return true;
        }

        /// <summary>
        /// Writes a structure of type T into a span of bytes.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(Span<byte> destination, ref T value)
            where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            }
            if ((uint)Unsafe.SizeOf<T>() > (uint)destination.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            }
            Unsafe.WriteUnaligned<T>(ref MemoryMarshal.GetReference<byte>(destination), value);
        }

        /// <summary>
        /// Writes a structure of type T into a span of bytes.
        /// <returns>If the span is too small to contain the type T, return false.</returns>
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWrite<T>(Span<byte> destination, ref T value)
            where T : struct
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            }
            if (Unsafe.SizeOf<T>() > (uint)destination.Length)
            {
                return false;
            }
            Unsafe.WriteUnaligned<T>(ref GetReference(destination), value);
            return true;
        }

        /// <summary>
        /// Creates a new memory over the portion of the pre-pinned target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The pre-pinned target array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <remarks>This method should only be called on an array that is already pinned and
        /// that array should not be unpinned while the returned Memory<typeparamref name="T"/> is still in use.
        /// Calling this method on an unpinned array could result in memory corruption.</remarks>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArrayTypeMismatchException">Thrown when <paramref name="array"/> is covariant and array's type is not exactly T[].</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;=Length).
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> CreateFromPinnedArray<T>(T[]? array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                return default;
            }
            if (!typeof(T).IsValueType && array.GetType() != typeof(T[]))
                ThrowHelper.ThrowArrayTypeMismatchException();
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();
            return new Memory<T>((object)array, start, length);
        }
    }
}
