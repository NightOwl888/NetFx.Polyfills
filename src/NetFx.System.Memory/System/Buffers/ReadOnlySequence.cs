// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    /// <summary>
    /// Represents a sequence that can read a sequential series of <typeparam name="T" />.
    /// </summary>
    [DebuggerTypeProxy(typeof(ReadOnlySequenceDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly partial struct ReadOnlySequence<T>
    {
        /// <summary>
        /// An enumerator over the <see cref="ReadOnlySequence{T}"/>
        /// </summary>
        public struct Enumerator
        {
            private readonly ReadOnlySequence<T> _sequence;
            private SequencePosition _next;
            private ReadOnlyMemory<T> _currentMemory;

            /// <summary>
            /// The current <see cref="ReadOnlyMemory{T}"/>
            /// </summary>
            public ReadOnlyMemory<T> Current => _currentMemory;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="sequence">The <see cref="ReadOnlySequence{T}"/> to enumerate.</param>
            public Enumerator(in ReadOnlySequence<T> sequence)
            {
                _currentMemory = default;
                _next = sequence.Start;
                _sequence = sequence;
            }

            /// <summary>
            /// Moves to the next <see cref="ReadOnlyMemory{T}"/> in the <see cref="ReadOnlySequence{T}"/>
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_next.GetObject() == null)
                {
                    return false;
                }

                return _sequence.TryGet(ref _next, out _currentMemory);
            }
        }

        private enum SequenceType
        {
            MultiSegment,
            Array,
            MemoryManager,
            String,
            Empty
        }

        private readonly SequencePosition _sequenceStart;

        private readonly SequencePosition _sequenceEnd;

        /// <summary>
        /// Returns empty <see cref="ReadOnlySequence{T}"/>
        /// </summary>
        public static readonly ReadOnlySequence<T> Empty = new ReadOnlySequence<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);

        /// <summary>
        /// Length of the <see cref="ReadOnlySequence{T}"/>.
        /// </summary>
        public long Length => GetLength();

        /// <summary>
        /// Determines if the <see cref="ReadOnlySequence{T}"/> is empty.
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// Determines if the <see cref="ReadOnlySequence{T}"/> contains a single <see cref="ReadOnlyMemory{T}"/> segment.
        /// </summary>
        public bool IsSingleSegment
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _sequenceStart.GetObject() == _sequenceEnd.GetObject();
            }
        }

        /// <summary>
        /// Gets <see cref="ReadOnlyMemory{T}"/> from the first segment.
        /// </summary>
        public ReadOnlyMemory<T> First => GetFirstBuffer();

        /// <summary>
        /// A position to the start of the <see cref="ReadOnlySequence{T}"/>.
        /// </summary>
        public SequencePosition Start => _sequenceStart;

        /// <summary>
        /// A position to the end of the <see cref="ReadOnlySequence{T}"/>
        /// </summary>
        public SequencePosition End => _sequenceEnd;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySequence(object? startSegment, int startIndexAndFlags, object? endSegment, int endIndexAndFlags)
        {
            // Used by SliceImpl to create new ReadOnlySequence

            // startSegment and endSegment can be null for default ReadOnlySequence only
            Debug.Assert((startSegment != null && endSegment != null) ||
                (startSegment == null && endSegment == null && startIndexAndFlags == 0 && endIndexAndFlags == 0));

            _sequenceStart = new SequencePosition(startSegment, startIndexAndFlags);
            _sequenceEnd = new SequencePosition(endSegment, endIndexAndFlags);
        }

        /// <summary>
        /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from linked memory list represented by start and end segments
        /// and corresponding indexes in them.
        /// </summary>
        public ReadOnlySequence(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment, int endIndex)
        {
            if (startSegment == null ||
                endSegment == null ||
                (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex) ||
                (uint)startSegment.Memory.Length < (uint)startIndex ||
                (uint)endSegment.Memory.Length < (uint)endIndex ||
                (startSegment == endSegment && endIndex < startIndex))
            {
                ThrowHelper.ThrowArgumentValidationException(startSegment, startIndex, endSegment);
            }

            _sequenceStart = new SequencePosition(startSegment, ReadOnlySequence.SegmentToSequenceStart(startIndex));
            _sequenceEnd = new SequencePosition(endSegment, ReadOnlySequence.SegmentToSequenceEnd(endIndex));
        }

        /// <summary>
        /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from the array.
        /// </summary>
        public ReadOnlySequence(T[] array)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(0));
            _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(array!.Length));
        }

        /// <summary>
        /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from the array, start, and index.
        /// </summary>
        public ReadOnlySequence(T[] array, int start, int length)
        {
            if (array == null || (uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
            {
                ThrowHelper.ThrowArgumentValidationException(array, start);
            }

            _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(start));
            _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(start + length));
        }

        /// <summary>
        /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from the <see cref="ReadOnlyMemory{T}"/>.
        /// Consumer is expected to manage lifetime of memory until <see cref="ReadOnlySequence{T}"/> is not used anymore.
        /// </summary>
        public ReadOnlySequence(ReadOnlyMemory<T> memory)
        {
            if (MemoryMarshal.TryGetMemoryManager<T, MemoryManager<T>>(memory, out MemoryManager<T>? manager, out int start, out int length))
            {
                _sequenceStart = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceStart(start));
                _sequenceEnd = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceEnd(start + length));
            }
            else if (MemoryMarshal.TryGetArray(memory, out ArraySegment<T> segment))
            {
                T[]? array = segment.Array;
                int offset = segment.Offset;
                _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(offset));
                _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(offset + segment.Count));
            }
            else if ((object)typeof(T) == typeof(char))
            {
                if (!MemoryMarshal.TryGetString((ReadOnlyMemory<char>)(object)memory, out string? text, out int start2, out length))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }

                _sequenceStart = new SequencePosition(text, ReadOnlySequence.StringToSequenceStart(start2));
                _sequenceEnd = new SequencePosition(text, ReadOnlySequence.StringToSequenceEnd(start2 + length));
            }
            else
            {
                ThrowHelper.ThrowInvalidOperationException();
                _sequenceStart = default;
                _sequenceEnd = default;
            }
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice that consists of <paramref name="length" /> elements from the current instance starting at index <paramref name="start" />.</returns>
        public ReadOnlySequence<T> Slice(long start, long length)
        {
            if (start < 0 || length < 0)
            {
                ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
            }

            int index = GetIndex(in _sequenceStart);
            int index2 = GetIndex(in _sequenceEnd);
            object? startObject = _sequenceStart.GetObject();
            object? endObject = _sequenceEnd.GetObject();
            SequencePosition start2;
            SequencePosition end;
            if (startObject != endObject)
            {
                Debug.Assert(startObject != null);
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)startObject!;
                int num = readOnlySequenceSegment.Memory.Length - index;
                if (num > start)
                {
                    index += (int)start;
                    start2 = new SequencePosition(startObject, index);
                    end = GetEndPosition(readOnlySequenceSegment, startObject!, index, endObject!, index2, length);
                }
                else
                {
                    if (num < 0)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }

                    start2 = SeekMultiSegment(readOnlySequenceSegment.Next!, endObject!, index2, start - num, ExceptionArgument.start);
                    int index3 = GetIndex(in start2);
                    object object3 = start2.GetObject()!;
                    if (object3 != endObject)
                    {
                        end = GetEndPosition((ReadOnlySequenceSegment<T>)object3, object3, index3, endObject!, index2, length);
                    }
                    else
                    {
                        if (index2 - index3 < length)
                        {
                            ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                        }

                        end = new SequencePosition(object3, index3 + (int)length);
                    }
                }
            }
            else
            {
                if (index2 - index < start)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
                }

                index += (int)start;
                start2 = new SequencePosition(startObject, index);
                if (index2 - index < length)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                }

                end = new SequencePosition(startObject, index + (int)length);
            }

            return SliceImpl(in start2, in end);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/> and ending at <paramref name="end"/> (exclusive).
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="end">The ending (exclusive) <see cref="SequencePosition"/> of the slice.</param>
        /// <returns>A slice that consists of items from the <paramref name="start" /> index to, but not including, the <paramref name="end" /> sequence position in the current read-only sequence.</returns>
        public ReadOnlySequence<T> Slice(long start, SequencePosition end)
        {
            if (start < 0)
            {
                ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
            }

            uint sliceEndIndex = (uint)GetIndex(in end);
            object? sliceEndObject = end.GetObject();

            uint startIndex = (uint)GetIndex(in _sequenceStart);
            object? startObject = _sequenceStart.GetObject();

            uint endIndex = (uint)GetIndex(in _sequenceEnd);
            object? endObject = _sequenceEnd.GetObject();

            if (sliceEndObject == null)
            {
                sliceEndObject = startObject;
                sliceEndIndex = startIndex;
            }

            if (startObject == endObject)
            {
                if (!InRange(sliceEndIndex, startIndex, endIndex))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                if (sliceEndIndex - startIndex < start)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
                }
            }
            else
            {
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)startObject!;
                ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + startIndex);
                ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)sliceEndObject!).RunningIndex + sliceEndIndex);
                if (!InRange(num2, num, (ulong)(((ReadOnlySequenceSegment<T>)endObject!).RunningIndex + endIndex)))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                if ((ulong)((long)num + start) > num2)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.start);
                }

                int num3 = readOnlySequenceSegment.Memory.Length - (int)startIndex;
                if (num3 <= start)
                {
                    if (num3 < 0)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }

                    SequencePosition start2 = SeekMultiSegment(readOnlySequenceSegment.Next!, sliceEndObject, (int)sliceEndIndex, start - num3, ExceptionArgument.start);
                    return SliceImpl(in start2, in end);
                }
            }

            // startIndex + start <= int.MaxValue
            Debug.Assert(start <= int.MaxValue - startIndex);

            SequencePosition start3 = new SequencePosition(startObject, (int)startIndex + (int)start);
            return SliceImpl(in start3, in end);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
        /// </summary>
        /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice that consists of <paramref name="length" /> elements from the current instance starting at sequence position <paramref name="start" />.</returns>
        public ReadOnlySequence<T> Slice(SequencePosition start, long length)
        {
            uint sliceStartIndex = (uint)GetIndex(in start);
            object? sliceStartObject = start.GetObject();

            uint startIndex = (uint)GetIndex(in _sequenceStart);
            object? startObject = _sequenceStart.GetObject();

            uint endIndex = (uint)GetIndex(in _sequenceEnd);
            object? endObject = _sequenceEnd.GetObject();

            if (sliceStartObject == null)
            {
                sliceStartIndex = startIndex;
                sliceStartObject = startObject;
            }

            if (startObject == endObject)
            {
                if (!InRange(sliceStartIndex, startIndex, endIndex))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                if (length < 0)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                }

                if (endIndex - sliceStartIndex < length)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                }
            }
            else
            {
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)sliceStartObject!;
                ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + sliceStartIndex);
                ulong start2 = (ulong)(((ReadOnlySequenceSegment<T>)startObject!).RunningIndex + startIndex);
                ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)endObject!).RunningIndex + endIndex);
                if (!InRange(num, start2, num2))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                if (length < 0)
                {
                    ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                }

                if ((ulong)((long)num + length) > num2)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
                }

                int num3 = readOnlySequenceSegment.Memory.Length - (int)sliceStartIndex;
                if (num3 < length)
                {
                    if (num3 < 0)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }

                    SequencePosition end = SeekMultiSegment(readOnlySequenceSegment.Next!, endObject, (int)endIndex, length - num3, ExceptionArgument.length);
                    return SliceImpl(in start, in end);
                }
            }

            SequencePosition end2 = new SequencePosition(sliceStartObject, (int)sliceStartIndex + (int)length);
            return SliceImpl(in start, in end2);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice that consists of <paramref name="length" /> elements from the current instance starting at index <paramref name="start" />.</returns>
        public ReadOnlySequence<T> Slice(int start, int length) => Slice((long)start, length);

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/> and ending at <paramref name="end"/> (exclusive).
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="end">The ending (exclusive) <see cref="SequencePosition"/> of the slice.</param>
        /// <returns>A slice that consists of items from the <paramref name="start" /> index to, but not including, the <paramref name="end" /> sequence position in the current read-only sequence.</returns>
        public ReadOnlySequence<T> Slice(int start, SequencePosition end) => Slice((long)start, end);

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
        /// </summary>
        /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A slice that consists of <paramref name="length" /> elements from the current instance starting at sequence position <paramref name="start" />.</returns>
        public ReadOnlySequence<T> Slice(SequencePosition start, int length) => Slice(start, (long)length);

        /// <summary>
        /// Forms a slice out of the given <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, ending at <paramref name="end"/> (exclusive).
        /// </summary>
        /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
        /// <param name="end">The ending (exclusive) <see cref="SequencePosition"/> of the slice.</param>
        /// <returns>A slice that consists of items from the <paramref name="start" /> sequence position to, but not including, the <paramref name="end" /> sequence position in the current read-only sequence.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySequence<T> Slice(SequencePosition start, SequencePosition end)
        {
            BoundsCheck((uint)GetIndex(in start), start.GetObject(), (uint)GetIndex(in end), end.GetObject());
            return SliceImpl(in start, in end);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}" />, beginning at a specified sequence position and continuing to the end of the read-only sequence.
        /// </summary>
        /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
        /// <returns>A slice starting at sequence position <paramref name="start" /> and continuing to the end of the current read-only sequence.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySequence<T> Slice(SequencePosition start)
        {
            BoundsCheck(in start);
            return SliceImpl(in start, in _sequenceEnd);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="ReadOnlySequence{T}" /> , beginning at a specified index and continuing to the end of the read-only sequence.
        /// </summary>
        /// <param name="start">The start index at which to begin this slice.</param>
        /// <returns>A slice starting at index <paramref name="start" /> and continuing to the end of the current read-only sequence.</returns>
        public ReadOnlySequence<T> Slice(long start)
        {
            if (start < 0)
            {
                ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
            }

            if (start == 0L)
            {
                return this;
            }

            SequencePosition start2 = Seek(in _sequenceStart, in _sequenceEnd, start, ExceptionArgument.start);
            return SliceImpl(in start2, in _sequenceEnd);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if ((object)typeof(T) == typeof(char))
            {
                ReadOnlySequence<T> source = this;
                ReadOnlySequence<char> sequence = Unsafe.As<ReadOnlySequence<T>, ReadOnlySequence<char>>(ref source);
                if (SequenceMarshal.TryGetString(sequence, out var text, out var start, out var length))
                {
                    return text.Substring(start, length);
                }

                if (Length < int.MaxValue)
                {
                    return new string(BuffersExtensions.ToArray(in sequence));
                }
            }

            return string.Format("System.Buffers.ReadOnlySequence<{0}>[{1}]", new object[2]
            {
            typeof(T).Name,
            Length
            });
        }

        /// <summary>
        /// Returns an enumerator over the <see cref="ReadOnlySequence{T}"/>
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Returns a new <see cref="SequencePosition"/> at an <paramref name="offset"/> from the start of the sequence.
        /// </summary>
        public SequencePosition GetPosition(long offset)
        {
            return GetPosition(offset, _sequenceStart);
        }

        /// <summary>
        /// Returns a new <see cref="SequencePosition"/> at an <paramref name="offset"/> from the <paramref name="origin"/>
        /// </summary>
        public SequencePosition GetPosition(long offset, SequencePosition origin)
        {
            if (offset < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_OffsetOutOfRange();
            }

            return Seek(in origin, in _sequenceEnd, offset, ExceptionArgument.offset);
        }

        /// <summary>
        /// Tries to retrieve next segment after <paramref name="position"/> and return its contents in <paramref name="memory"/>.
        /// Returns <code>false</code> if end of <see cref="ReadOnlySequence{T}"/> was reached otherwise <code>true</code>.
        /// Sets <paramref name="position"/> to the beginning of next segment if <paramref name="advance"/> is set to <code>true</code>.
        /// </summary>
        public bool TryGet(ref SequencePosition position, out ReadOnlyMemory<T> memory, bool advance = true)
        {
            bool result = TryGetBuffer(in position, out memory, out SequencePosition next);
            if (advance)
            {
                position = next;
            }

            return result;
        }



        

        



        private void BoundsCheck(in SequencePosition position)
        {
            uint index = (uint)GetIndex(in position);
            uint index2 = (uint)GetIndex(in _sequenceStart);
            uint index3 = (uint)GetIndex(in _sequenceEnd);
            object? @object = _sequenceStart.GetObject();
            object? object2 = _sequenceEnd.GetObject();
            if (@object == object2)
            {
                if (!InRange(index, index2, index3))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                return;
            }

            ulong start = (ulong)(((ReadOnlySequenceSegment<T>)@object!).RunningIndex + index2);
            if (!InRange((ulong)(((ReadOnlySequenceSegment<T>)position.GetObject()!).RunningIndex + index), start, (ulong)(((ReadOnlySequenceSegment<T>)object2!).RunningIndex + index3)))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
            }
        }
    }

    internal static class ReadOnlySequence
    {
        /// <summary>
        /// Flag that allows encoding the <see cref="ReadOnlySequence{T}.SequenceType"/>.
        /// </summary>
        /// <seealso cref="ReadOnlySequence{T}.GetSequenceType"/>
        public const int FlagBitMask = 1 << 31;
        public const int IndexBitMask = ~FlagBitMask;

        public const int SegmentStartMask = 0;
        public const int SegmentEndMask = 0;

        public const int ArrayStartMask = 0;
        public const int ArrayEndMask = FlagBitMask;

        public const int MemoryManagerStartMask = FlagBitMask;
        public const int MemoryManagerEndMask = 0;

        public const int StringStartMask = FlagBitMask;
        public const int StringEndMask = FlagBitMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SegmentToSequenceStart(int startIndex) => startIndex | SegmentStartMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SegmentToSequenceEnd(int endIndex) => endIndex | SegmentEndMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArrayToSequenceStart(int startIndex) => startIndex | ArrayStartMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ArrayToSequenceEnd(int endIndex) => endIndex | ArrayEndMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MemoryManagerToSequenceStart(int startIndex) => startIndex | MemoryManagerStartMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MemoryManagerToSequenceEnd(int endIndex) => endIndex | MemoryManagerEndMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StringToSequenceStart(int startIndex) => startIndex | StringStartMask;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StringToSequenceEnd(int endIndex) => endIndex | StringEndMask;
    }
}