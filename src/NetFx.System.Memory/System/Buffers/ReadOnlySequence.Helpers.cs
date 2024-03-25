// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Buffers
{

    public readonly partial struct ReadOnlySequence<T>
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetBuffer(in SequencePosition position, out ReadOnlyMemory<T> memory, out SequencePosition next)
        {
            object? @object = position.GetObject();
            next = default;
            if (@object == null)
            {
                memory = default;
                return false;
            }

            SequenceType sequenceType = GetSequenceType();
            object? object2 = _sequenceEnd.GetObject();
            int index = GetIndex(in position);
            int index2 = GetIndex(in _sequenceEnd);
            if (sequenceType == SequenceType.MultiSegment)
            {
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                if (readOnlySequenceSegment != object2)
                {
                    ReadOnlySequenceSegment<T> next2 = readOnlySequenceSegment.Next!;
                    if (next2 == null)
                    {
                        ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                    }

                    next = new SequencePosition(next2, 0);
                    memory = readOnlySequenceSegment.Memory.Slice(index);
                }
                else
                {
                    memory = readOnlySequenceSegment.Memory.Slice(index, index2 - index);
                }
            }
            else
            {
                if (@object != object2)
                {
                    ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                }

                if (sequenceType == SequenceType.Array)
                {
                    memory = new ReadOnlyMemory<T>((T[])@object, index, index2 - index);
                }
                else if ((object)typeof(T) == typeof(char) && sequenceType == SequenceType.String)
                {
                    memory = (ReadOnlyMemory<T>)(object)((string)@object).AsMemory(index, index2 - index);
                }
                else
                {
                    memory = ((MemoryManager<T>)@object).Memory.Slice(index, index2 - index);
                }
            }

            return true;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory<T> GetFirstBuffer()
        {
            object? @object = _sequenceStart.GetObject();
            if (@object == null)
            {
                return default;
            }

            int integer = _sequenceStart.GetInteger();
            int integer2 = _sequenceEnd.GetInteger();
            bool flag = @object != _sequenceEnd.GetObject();
            if (integer >= 0)
            {
                if (integer2 >= 0)
                {
                    ReadOnlyMemory<T> memory = ((ReadOnlySequenceSegment<T>)@object).Memory;
                    if (flag)
                    {
                        return memory.Slice(integer);
                    }

                    return memory.Slice(integer, integer2 - integer);
                }

                if (flag)
                {
                    ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                }

                return new ReadOnlyMemory<T>((T[])@object, integer, (integer2 & 0x7FFFFFFF) - integer);
            }

            if (flag)
            {
                ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
            }

            if ((object)typeof(T) == typeof(char) && integer2 < 0)
            {
                return (ReadOnlyMemory<T>)(object)((string)@object).AsMemory(integer & 0x7FFFFFFF, integer2 - integer);
            }

            integer &= 0x7FFFFFFF;
            return ((MemoryManager<T>)@object).Memory.Slice(integer, integer2 - integer);
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SequencePosition Seek(in SequencePosition start, in SequencePosition end, long offset, ExceptionArgument argument)
        {
            int startIndex = GetIndex(in start);
            int endIndex = GetIndex(in end);
            object? startObject = start.GetObject();
            object? endObject = end.GetObject();
            if (startObject != endObject)
            {
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)startObject!;
                int num = readOnlySequenceSegment.Memory.Length - startIndex;
                if (num <= offset)
                {
                    if (num < 0)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }

                    return SeekMultiSegment(readOnlySequenceSegment.Next!, endObject!, endIndex, offset - num, argument);
                }
            }
            else if (endIndex - startIndex < offset)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(argument);
            }

            return new SequencePosition(startObject, startIndex + (int)offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static SequencePosition SeekMultiSegment(ReadOnlySequenceSegment<T>? currentSegment, object endObject, int endIndex, long offset, System.ExceptionArgument argument)
        {
            while (true)
            {
                if (currentSegment != null && currentSegment != endObject)
                {
                    int length = currentSegment.Memory.Length;
                    if (length > offset)
                    {
                        break;
                    }

                    offset -= length;
                    currentSegment = currentSegment.Next;
                    continue;
                }

                if (currentSegment == null || endIndex < offset)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(argument);
                }

                break;
            }

            return new SequencePosition(currentSegment, (int)offset);
        }

        private void BoundsCheck(uint sliceStartIndex, object? sliceStartObject, uint sliceEndIndex, object? sliceEndObject)
        {
            uint index = (uint)GetIndex(in _sequenceStart);
            uint index2 = (uint)GetIndex(in _sequenceEnd);
            object? @object = _sequenceStart.GetObject();
            object? object2 = _sequenceEnd.GetObject();
            if (@object == object2)
            {
                if (sliceStartObject != sliceEndObject || sliceStartObject != @object || sliceStartIndex > sliceEndIndex || sliceStartIndex < index || sliceEndIndex > index2)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }

                return;
            }

            ulong num = (ulong)(((ReadOnlySequenceSegment<T>)sliceStartObject!).RunningIndex + sliceStartIndex);
            ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)sliceEndObject!).RunningIndex + sliceEndIndex);
            if (num > num2)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
            }

            if (num < (ulong)(((ReadOnlySequenceSegment<T>)@object!).RunningIndex + index) || num2 > (ulong)(((ReadOnlySequenceSegment<T>)object2!).RunningIndex + index2))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
            }
        }

        private static SequencePosition GetEndPosition(ReadOnlySequenceSegment<T> startSegment, object startObject, int startIndex, object endObject, int endIndex, long length)
        {
            int num = startSegment.Memory.Length - startIndex;
            if (num > length)
            {
                return new SequencePosition(startObject, startIndex + (int)length);
            }

            if (num < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
            }

            return SeekMultiSegment(startSegment.Next, endObject, endIndex, length - num, ExceptionArgument.length);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SequenceType GetSequenceType()
        {
            // We take high order bits of two indexes and move them
            // to a first and second position to convert to SequenceType

            // if (start < 0  and end < 0)
            // start >> 31 = -1, end >> 31 = -1
            // 2 * (-1) + (-1) = -3, result = (SequenceType)3

            // if (start < 0  and end >= 0)
            // start >> 31 = -1, end >> 31 = 0
            // 2 * (-1) + 0 = -2, result = (SequenceType)2

            // if (start >= 0  and end >= 0)
            // start >> 31 = 0, end >> 31 = 0
            // 2 * 0 + 0 = 0, result = (SequenceType)0

            // if (start >= 0  and end < 0)
            // start >> 31 = 0, end >> 31 = -1
            // 2 * 0 + (-1) = -1, result = (SequenceType)1

            return (SequenceType)(-(2 * (_sequenceStart.GetInteger() >> 31) + (_sequenceEnd.GetInteger() >> 31)));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetIndex(in SequencePosition position)
        {
            return position.GetInteger() & 0x7FFFFFFF;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySequence<T> SliceImpl(in SequencePosition start, in SequencePosition end)
        {
            return new ReadOnlySequence<T>(start.GetObject(), GetIndex(in start) | (_sequenceStart.GetInteger() & int.MinValue), end.GetObject(), GetIndex(in end) | (_sequenceEnd.GetInteger() & int.MinValue));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetLength()
        {
            int index = GetIndex(in _sequenceStart);
            int index2 = GetIndex(in _sequenceEnd);
            object? @object = _sequenceStart.GetObject();
            object? object2 = _sequenceEnd.GetObject();
            if (@object != object2)
            {
                ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object!;
                ReadOnlySequenceSegment<T> readOnlySequenceSegment2 = (ReadOnlySequenceSegment<T>)object2!;
                return readOnlySequenceSegment2.RunningIndex + index2 - (readOnlySequenceSegment.RunningIndex + index);
            }

            return index2 - index;
        }

        internal bool TryGetReadOnlySequenceSegment([NotNullWhen(true)] out ReadOnlySequenceSegment<T>? startSegment, out int startIndex, [NotNullWhen(true)] out ReadOnlySequenceSegment<T>? endSegment, out int endIndex)
        {
            object? @object = _sequenceStart.GetObject();
            if (@object == null || GetSequenceType() != 0)
            {
                startSegment = null;
                startIndex = 0;
                endSegment = null;
                endIndex = 0;
                return false;
            }

            object? endObject = _sequenceEnd.GetObject();
            Debug.Assert(endObject != null);

            startSegment = (ReadOnlySequenceSegment<T>)@object;
            startIndex = GetIndex(in _sequenceStart);
            endSegment = (ReadOnlySequenceSegment<T>)endObject!;
            endIndex = GetIndex(in _sequenceEnd);
            return true;
        }

        internal bool TryGetArray(out ArraySegment<T> segment)
        {
            if (GetSequenceType() != SequenceType.Array)
            {
                segment = default;
                return false;
            }

            object? startObject = _sequenceStart.GetObject();
            Debug.Assert(startObject != null);

            int index = GetIndex(in _sequenceStart);
            segment = new ArraySegment<T>((T[])startObject!, index, GetIndex(in _sequenceEnd) - index);
            return true;
        }

        internal bool TryGetString([NotNullWhen(true)] out string? text, out int start, out int length)
        {
            if ((object)typeof(T) != typeof(char) || GetSequenceType() != SequenceType.String)
            {
                start = 0;
                length = 0;
                text = null;
                return false;
            }

            object? startObject = _sequenceStart.GetObject();
            Debug.Assert(startObject != null);

            start = GetIndex(in _sequenceStart);
            length = GetIndex(in _sequenceEnd) - start;
            text = (string)startObject!;
            return true;
        }

        private static bool InRange(uint value, uint start, uint end)
        {
            // _sequenceStart and _sequenceEnd must be well-formed
            Debug.Assert(start <= int.MaxValue);
            Debug.Assert(end <= int.MaxValue);
            Debug.Assert(start <= end);

            // The case, value > int.MaxValue, is invalid, and hence it shouldn't be in the range.
            // If value > int.MaxValue, it is invariably greater than both 'start' and 'end'.
            // In that case, the experession simplifies to value <= end, which will return false.

            // The case, value < start, is invalid.
            // In that case, (value - start) would underflow becoming larger than int.MaxValue.
            // (end - start) can never underflow and hence must be within 0 and int.MaxValue.
            // So, we will correctly return false.

            // The case, value > end, is invalid.
            // In that case, the expression simplifies to value <= end, which will return false.
            // This is because end > start & value > end implies value > start as well.

            // In all other cases, value is valid, and we return true.

            // Equivalent to: return (start <= value && value <= end)
            return value - start <= end - start;
        }

        private static bool InRange(ulong value, ulong start, ulong end)
        {
            // _sequenceStart and _sequenceEnd must be well-formed
            Debug.Assert(start <= long.MaxValue);
            Debug.Assert(end <= long.MaxValue);
            Debug.Assert(start <= end);

            // The case, value > long.MaxValue, is invalid, and hence it shouldn't be in the range.
            // If value > long.MaxValue, it is invariably greater than both 'start' and 'end'.
            // In that case, the experession simplifies to value <= end, which will return false.

            // The case, value < start, is invalid.
            // In that case, (value - start) would underflow becoming larger than long.MaxValue.
            // (end - start) can never underflow and hence must be within 0 and long.MaxValue.
            // So, we will correctly return false.

            // The case, value > end, is invalid.
            // In that case, the expression simplifies to value <= end, which will return false.
            // This is because end > start & value > end implies value > start as well.

            // In all other cases, value is valid, and we return true.

            // Equivalent to: return (start <= value && value <= start)
            return value - start <= end - start;
        }
    }
}