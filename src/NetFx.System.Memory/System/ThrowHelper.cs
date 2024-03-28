// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Decompiled with JetBrains decompiler
// Type: System.ThrowHelper
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// This file defines an internal static class used to throw exceptions in BCL code.
// The main purpose is to reduce code size.
//
// The old way to throw an exception generates quite a lot IL code and assembly code.
// Following is an example:
//     C# source
//          throw new ArgumentNullException(nameof(key), SR.ArgumentNull_Key);
//     IL code:
//          IL_0003:  ldstr      "key"
//          IL_0008:  ldstr      "ArgumentNull_Key"
//          IL_000d:  call       string System.Environment::GetResourceString(string)
//          IL_0012:  newobj     instance void System.ArgumentNullException::.ctor(string,string)
//          IL_0017:  throw
//    which is 21bytes in IL.
//
// So we want to get rid of the ldstr and call to Environment.GetResource in IL.
// In order to do that, I created two enums: ExceptionResource, ExceptionArgument to represent the
// argument name and resource name in a small integer. The source code will be changed to
//    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key, ExceptionResource.ArgumentNull_Key);
//
// The IL code will be 7 bytes.
//    IL_0008:  ldc.i4.4
//    IL_0009:  ldc.i4.4
//    IL_000a:  call       void System.ThrowHelper::ThrowArgumentNullException(valuetype System.ExceptionArgument)
//    IL_000f:  ldarg.0
//
// This will also reduce the Jitted code size a lot.
//
// It is very important we do this for generic classes because we can easily generate the same code
// multiple times for different instantiation.
//

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System
{
    internal static class ThrowHelper
    {
        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw CreateArgumentNullException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentNullException(ExceptionArgument argument)
        {
            return new ArgumentNullException(argument.ToString());
        }

        internal static void ThrowArrayTypeMismatchException()
        {
            throw CreateArrayTypeMismatchException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArrayTypeMismatchException()
        {
            return new ArrayTypeMismatchException();
        }

        internal static void ThrowArgumentException_InvalidTypeWithPointersNotSupported(Type type)
        {
            throw CreateArgumentException_InvalidTypeWithPointersNotSupported(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_InvalidTypeWithPointersNotSupported(Type type)
        {
            return new ArgumentException(SR2.Format(SR.Argument_InvalidTypeWithPointersNotSupported, type));
        }

        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw CreateArgumentException_DestinationTooShort();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_DestinationTooShort()
        {
            return new ArgumentException(SR.Argument_DestinationTooShort);
        }

        internal static void ThrowIndexOutOfRangeException()
        {
            throw CreateIndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateIndexOutOfRangeException()
        {
            return new IndexOutOfRangeException();
        }

        internal static void ThrowArgumentOutOfRangeException()
        {
            throw CreateArgumentOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException()
        {
            return new ArgumentOutOfRangeException();
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw CreateArgumentOutOfRangeException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException(ExceptionArgument argument)
        {
            return new ArgumentOutOfRangeException(argument.ToString());
        }

        internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge()
        {
            throw CreateArgumentOutOfRangeException_PrecisionTooLarge();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PrecisionTooLarge()
        {
            return new ArgumentOutOfRangeException("precision", SR2.Format(SR.Argument_PrecisionTooLarge, (byte)99));
        }

        internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            throw CreateArgumentOutOfRangeException_SymbolDoesNotFit();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            return new ArgumentOutOfRangeException("symbol", SR.Argument_BadFormatSpecifier);
        }

        internal static void ThrowInvalidOperationException()
        {
            throw CreateInvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException()
        {
            return new InvalidOperationException();
        }

        internal static void ThrowInvalidOperationException_OutstandingReferences()
        {
            throw CreateInvalidOperationException_OutstandingReferences();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_OutstandingReferences()
        {
            return new InvalidOperationException(SR.OutstandingReferences);
        }

        internal static void ThrowInvalidOperationException_UnexpectedSegmentType()
        {
            throw CreateInvalidOperationException_UnexpectedSegmentType();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_UnexpectedSegmentType()
        {
            return new InvalidOperationException(SR.UnexpectedSegmentType);
        }

        internal static void ThrowInvalidOperationException_EndPositionNotReached()
        {
            throw CreateInvalidOperationException_EndPositionNotReached();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_EndPositionNotReached()
        {
            return new InvalidOperationException(SR.EndPositionNotReached);
        }

        internal static void ThrowArgumentOutOfRangeException_PositionOutOfRange()
        {
            throw CreateArgumentOutOfRangeException_PositionOutOfRange();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PositionOutOfRange()
        {
            return new ArgumentOutOfRangeException("position");
        }

        internal static void ThrowArgumentOutOfRangeException_OffsetOutOfRange()
        {
            throw CreateArgumentOutOfRangeException_OffsetOutOfRange();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_OffsetOutOfRange()
        {
            return new ArgumentOutOfRangeException("offset");
        }

        internal static void ThrowObjectDisposedException_ArrayMemoryPoolBuffer()
        {
            throw CreateObjectDisposedException_ArrayMemoryPoolBuffer();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateObjectDisposedException_ArrayMemoryPoolBuffer()
        {
            return new ObjectDisposedException("ArrayMemoryPoolBuffer");
        }

        internal static void ThrowFormatException_BadFormatSpecifier()
        {
            throw CreateFormatException_BadFormatSpecifier();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateFormatException_BadFormatSpecifier()
        {
            return new FormatException(SR.Argument_BadFormatSpecifier);
        }

        internal static void ThrowArgumentException_OverlapAlignmentMismatch()
        {
            throw CreateArgumentException_OverlapAlignmentMismatch();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_OverlapAlignmentMismatch()
        {
            return new ArgumentException(SR.Argument_OverlapAlignmentMismatch);
        }

        internal static void ThrowNotSupportedException()
        {
            throw CreateThrowNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateThrowNotSupportedException()
        {
            return new NotSupportedException();
        }

        public static bool TryFormatThrowFormatException(out int bytesWritten)
        {
            bytesWritten = 0;
            ThrowFormatException_BadFormatSpecifier();
            return false;
        }

        public static bool TryParseThrowFormatException<T>([MaybeNullWhen(false)] out T? value, out int bytesConsumed)
        {
            value = default;
            bytesConsumed = 0;
            ThrowFormatException_BadFormatSpecifier();
            return false;
        }

        public static void ThrowArgumentValidationException<T>(
            ReadOnlySequenceSegment<T>? startSegment,
            int startIndex,
            ReadOnlySequenceSegment<T>? endSegment)
        {
            throw CreateArgumentValidationException(startSegment, startIndex, endSegment);
        }

        private static Exception CreateArgumentValidationException<T>(
            ReadOnlySequenceSegment<T>? startSegment,
            int startIndex,
            ReadOnlySequenceSegment<T>? endSegment)
        {
            if (startSegment == null)
                return CreateArgumentNullException(ExceptionArgument.startSegment);
            if (endSegment == null)
                return CreateArgumentNullException(ExceptionArgument.endSegment);
            if (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex)
                return CreateArgumentOutOfRangeException(ExceptionArgument.endSegment);
            return (uint)startSegment.Memory.Length < (uint)startIndex
                ? CreateArgumentOutOfRangeException(ExceptionArgument.startIndex)
                : CreateArgumentOutOfRangeException(ExceptionArgument.endIndex);
        }

        public static void ThrowArgumentValidationException(Array? array, int start)
        {
            throw CreateArgumentValidationException(array, start);
        }

        private static Exception CreateArgumentValidationException(Array? array, int start)
        {
            if (array == null)
                return CreateArgumentNullException(ExceptionArgument.array);
            return (uint)start > (uint)array.Length
                ? CreateArgumentOutOfRangeException(ExceptionArgument.start)
                : CreateArgumentOutOfRangeException(ExceptionArgument.length);
        }

        public static void ThrowStartOrEndArgumentValidationException(long start)
        {
            throw CreateStartOrEndArgumentValidationException(start);
        }

        private static Exception CreateStartOrEndArgumentValidationException(long start)
        {
            return start < 0L
                ? CreateArgumentOutOfRangeException(ExceptionArgument.start)
                : CreateArgumentOutOfRangeException(ExceptionArgument.length);
        }
    }
}
