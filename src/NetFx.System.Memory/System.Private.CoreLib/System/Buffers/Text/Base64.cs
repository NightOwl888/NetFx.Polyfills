// Decompiled with JetBrains decompiler
// Type: System.Buffers.Text.Base64
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers.Text
{
    public static class Base64
    {
        private static readonly sbyte[] s_decodingMap = new sbyte[256]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
            54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
            -1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
            5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
            29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
            39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
            49, 50, 51, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1
        };

        private static readonly byte[] s_encodingMap = new byte[64]
        {
            65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
            75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
            85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
            101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
            111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
            121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
            56, 57, 43, 47
        };

        private const byte EncodingPad = 61;

        private const int MaximumEncodeLength = 1610612733;

        /// <summary>
        /// Decode the span of UTF-8 encoded text represented as base64 into binary data.
        /// If the input is not a multiple of 4, it will decode as much as it can, to the closest multiple of 4.
        /// </summary>
        /// <param name="utf8">The input span which contains UTF-8 encoded text in base64 that needs to be decoded.</param>
        /// <param name="bytes">The output span which contains the result of the operation, i.e. the decoded binary data.</param>
        /// <param name="bytesConsumed">The number of input bytes consumed during the operation. This can be used to slice the input for subsequent calls, if necessary.</param>
        /// <param name="bytesWritten">The number of bytes written into the output span. This can be used to slice the output for subsequent calls, if necessary.</param>
        /// <param name="isFinalBlock"><see langword="true"/> (default) when the input span contains the entire data to encode.
        /// Set to <see langword="true"/> when the source buffer contains the entirety of the data to encode.
        /// Set to <see langword="false"/> if this method is being called in a loop and if more input data may follow.
        /// At the end of the loop, call this (potentially with an empty source buffer) passing <see langword="true"/>.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire input span
        /// - DestinationTooSmall - if there is not enough space in the output span to fit the decoded input
        /// - NeedMoreData - only if <paramref name="isFinalBlock"/> is false and the input is not a multiple of 4, otherwise the partial input would be considered as InvalidData
        /// - InvalidData - if the input contains bytes outside of the expected base64 range, or if it contains invalid/more than two padding characters,
        ///   or if the input is incomplete (i.e. not a multiple of 4) and <paramref name="isFinalBlock"/> is <see langword="true"/>.
        /// </returns>
        public static unsafe OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> utf8, Span<byte> bytes, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
        {
            ref byte local1 = ref MemoryMarshal.GetReference(utf8);
            ref byte local2 = ref MemoryMarshal.GetReference(bytes);
            int length1 = utf8.Length & -4;
            int length2 = bytes.Length;
            int elementOffset1 = 0;
            int elementOffset2 = 0;
            if (utf8.Length != 0)
            {
                ref sbyte local3 = ref s_decodingMap[0];
                int num1 = isFinalBlock ? 4 : 0;
                int num2;
                for (num2 = length2 < GetMaxDecodedFromUtf8Length(length1) ? length2 / 3 * 4 : length1 - num1; elementOffset1 < num2; elementOffset1 += 4)
                {
                    int num3 = Decode(ref Unsafe.Add(ref local1, elementOffset1), ref local3);
                    if (num3 >= 0)
                    {
                        WriteThreeLowOrderBytes(ref Unsafe.Add(ref local2, elementOffset2), num3);
                        elementOffset2 += 3;
                    }
                    else
                        goto label_24;
                }
                if (num2 == length1 - num1)
                {
                    if (elementOffset1 == length1)
                    {
                        if (!isFinalBlock)
                        {
                            bytesConsumed = elementOffset1;
                            bytesWritten = elementOffset2;
                            return OperationStatus.NeedMoreData;
                        }
                        goto label_24;
                    }
                    else
                    {
                        int elementOffset3 = Unsafe.Add(ref local1, length1 - 4);
                        int elementOffset4 = Unsafe.Add(ref local1, length1 - 3);
                        int elementOffset5 = Unsafe.Add(ref local1, length1 - 2);
                        int elementOffset6 = Unsafe.Add(ref local1, length1 - 1);
                        int num3 = Unsafe.Add(ref local3, elementOffset3) << 18 | Unsafe.Add(ref local3, elementOffset4) << 12;
                        if (elementOffset6 != EncodingPad)
                        {
                            int num4 = Unsafe.Add(ref local3, elementOffset5);
                            int num5 = Unsafe.Add(ref local3, elementOffset6);
                            int num6 = num4 << 6;
                            int num7 = num3 | num5 | num6;
                            if (num7 >= 0)
                            {
                                if (elementOffset2 <= length2 - 3)
                                {
                                    WriteThreeLowOrderBytes(ref Unsafe.Add(ref local2, elementOffset2), num7);
                                    elementOffset2 += 3;
                                }
                                else
                                    goto label_21;
                            }
                            else
                                goto label_24;
                        }
                        else if (elementOffset5 != EncodingPad)
                        {
                            int num4 = Unsafe.Add(ref local3, elementOffset5) << 6;
                            int num5 = num3 | num4;
                            if (num5 >= 0)
                            {
                                if (elementOffset2 <= length2 - 2)
                                {
                                    Unsafe.Add(ref local2, elementOffset2) = (byte)(num5 >> 16);
                                    Unsafe.Add(ref local2, elementOffset2 + 1) = (byte)(num5 >> 8);
                                    elementOffset2 += 2;
                                }
                                else
                                    goto label_21;
                            }
                            else
                                goto label_24;
                        }
                        else if (num3 >= 0)
                        {
                            if (elementOffset2 <= length2 - 1)
                            {
                                Unsafe.Add(ref local2, elementOffset2) = (byte)(num3 >> 16);
                                ++elementOffset2;
                            }
                            else
                                goto label_21;
                        }
                        else
                            goto label_24;
                        elementOffset1 += 4;
                        if (length1 != utf8.Length)
                            goto label_24;
                        else
                            goto label_20;
                    }
                }
            label_21:
                if (!(length1 != utf8.Length & isFinalBlock))
                {
                    bytesConsumed = elementOffset1;
                    bytesWritten = elementOffset2;
                    return OperationStatus.DestinationTooSmall;
                }
            label_24:
                bytesConsumed = elementOffset1;
                bytesWritten = elementOffset2;
                return OperationStatus.InvalidData;
            }
        label_20:
            bytesConsumed = elementOffset1;
            bytesWritten = elementOffset2;
            return OperationStatus.Done;
        }

        /// <summary>
        /// Returns the maximum length (in bytes) of the result if you were to deocde base 64 encoded text within a byte span of size "length".
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="length"/> is less than 0.
        /// </exception>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxDecodedFromUtf8Length(int length)
        {
            if (length < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            return (length >> 2) * 3;
        }

        /// <summary>
        /// Decode the span of UTF-8 encoded text in base 64 (in-place) into binary data.
        /// The decoded binary output is smaller than the text data contained in the input (the operation deflates the data).
        /// If the input is not a multiple of 4, it will not decode any.
        /// </summary>
        /// <param name="buffer">The input span which contains the base 64 text data that needs to be decoded.</param>
        /// <param name="bytesWritten">The number of bytes written into the buffer.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire input span
        /// - InvalidData - if the input contains bytes outside of the expected base 64 range, or if it contains invalid/more than two padding characters,
        ///   or if the input is incomplete (i.e. not a multiple of 4).
        /// It does not return DestinationTooSmall since that is not possible for base 64 decoding.
        /// It does not return NeedMoreData since this method tramples the data in the buffer and
        /// hence can only be called once with all the data in the buffer.
        /// </returns>
        public static OperationStatus DecodeFromUtf8InPlace(Span<byte> buffer,out int bytesWritten)
        {
            int length = buffer.Length;
            int elementOffset1 = 0;
            int elementOffset2 = 0;
            if (length == (length >> 2) * 4)
            {
                if (length != 0)
                {
                    ref byte local1 = ref MemoryMarshal.GetReference(buffer);
                    ref sbyte local2 = ref s_decodingMap[0];
                    for (; elementOffset1 < length - 4; elementOffset1 += 4)
                    {
                        int num = Decode(ref Unsafe.Add(ref local1, elementOffset1), ref local2);
                        if (num >= 0)
                        {
                            WriteThreeLowOrderBytes(ref Unsafe.Add(ref local1, elementOffset2), num);
                            elementOffset2 += 3;
                        }
                        else
                            goto label_15;
                    }
                    int elementOffset3 = Unsafe.Add(ref local1, length - 4);
                    int elementOffset4 = Unsafe.Add(ref local1, length - 3);
                    int elementOffset5 = Unsafe.Add(ref local1, length - 2);
                    int elementOffset6 = Unsafe.Add(ref local1, length - 1);
                    int num1 = Unsafe.Add(ref local2, elementOffset3) << 18 | Unsafe.Add(ref local2, elementOffset4) << 12;
                    if (elementOffset6 != EncodingPad)
                    {
                        int num2 = Unsafe.Add(ref local2, elementOffset5);
                        int num3 = Unsafe.Add(ref local2, elementOffset6);
                        int num4 = num2 << 6;
                        int num5 = num1 | num3 | num4;
                        if (num5 >= 0)
                        {
                            WriteThreeLowOrderBytes(ref Unsafe.Add(ref local1, elementOffset2), num5);
                            elementOffset2 += 3;
                        }
                        else
                            goto label_15;
                    }
                    else if (elementOffset5 != EncodingPad)
                    {
                        int num2 = Unsafe.Add(ref local2, elementOffset5) << 6;
                        int num3 = num1 | num2;
                        if (num3 >= 0)
                        {
                            Unsafe.Add(ref local1, elementOffset2) = (byte)(num3 >> 16);
                            Unsafe.Add(ref local1, elementOffset2 + 1) = (byte)(num3 >> 8);
                            elementOffset2 += 2;
                        }
                        else
                            goto label_15;
                    }
                    else if (num1 >= 0)
                    {
                        Unsafe.Add(ref local1, elementOffset2) = (byte)(num1 >> 16);
                        ++elementOffset2;
                    }
                    else
                        goto label_15;
                }
                bytesWritten = elementOffset2;
                return OperationStatus.Done;
            }
        label_15:
            bytesWritten = elementOffset2;
            return OperationStatus.InvalidData;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Decode(ref byte encodedBytes, ref sbyte decodingMap)
        {
            int elementOffset1 = encodedBytes;
            int elementOffset2 = Unsafe.Add(ref encodedBytes, 1);
            int elementOffset3 = Unsafe.Add(ref encodedBytes, 2);
            int elementOffset4 = Unsafe.Add(ref encodedBytes, 3);
            int num1 = Unsafe.Add(ref decodingMap, elementOffset1);
            int num2 = Unsafe.Add(ref decodingMap, elementOffset2);
            int num3 = Unsafe.Add(ref decodingMap, elementOffset3);
            int num4 = Unsafe.Add(ref decodingMap, elementOffset4);
            int num5 = num1 << 18;
            int num6 = num2 << 12;
            int num7 = num3 << 6;
            return num5 | num4 | (num6 | num7);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteThreeLowOrderBytes(ref byte destination, int value)
        {
            destination = (byte)(value >> 16);
            Unsafe.Add(ref destination, 1) = (byte)(value >> 8);
            Unsafe.Add(ref destination, 2) = (byte)value;
        }

        public static OperationStatus EncodeToUtf8(ReadOnlySpan<byte> bytes, Span<byte> utf8, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
        {
            ref byte local1 = ref MemoryMarshal.GetReference(bytes);
            ref byte local2 = ref MemoryMarshal.GetReference(utf8);
            int length1 = bytes.Length;
            int length2 = utf8.Length;
            int num1 = length1 > MaximumEncodeLength || length2 < GetMaxEncodedToUtf8Length(length1) ? (length2 >> 2) * 3 - 2 : length1 - 2;
            int elementOffset1 = 0;
            int elementOffset2 = 0;
            ref byte local3 = ref s_encodingMap[0];
            for (; elementOffset1 < num1; elementOffset1 += 3)
            {
                int num2 = Encode(ref Unsafe.Add(ref local1, elementOffset1), ref local3);
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref local2, elementOffset2), num2);
                elementOffset2 += 4;
            }
            if (num1 == length1 - 2)
            {
                if (isFinalBlock)
                {
                    if (elementOffset1 == length1 - 1)
                    {
                        int num2 = EncodeAndPadTwo(ref Unsafe.Add(ref local1, elementOffset1), ref local3);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref local2, elementOffset2), num2);
                        elementOffset2 += 4;
                        ++elementOffset1;
                    }
                    else if (elementOffset1 == length1 - 2)
                    {
                        int num2 = EncodeAndPadOne(ref Unsafe.Add(ref local1, elementOffset1), ref local3);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref local2, elementOffset2), num2);
                        elementOffset2 += 4;
                        elementOffset1 += 2;
                    }
                    bytesConsumed = elementOffset1;
                    bytesWritten = elementOffset2;
                    return OperationStatus.Done;
                }
                bytesConsumed = elementOffset1;
                bytesWritten = elementOffset2;
                return OperationStatus.NeedMoreData;
            }
            bytesConsumed = elementOffset1;
            bytesWritten = elementOffset2;
            return OperationStatus.DestinationTooSmall;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxEncodedToUtf8Length(int length)
        {
            if ((uint)length > 1610612733U)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);
            return (length + 2) / 3 * 4;
        }

        public static OperationStatus EncodeToUtf8InPlace(
          Span<byte> buffer,
          int dataLength,
          out int bytesWritten)
        {
            int encodedToUtf8Length = GetMaxEncodedToUtf8Length(dataLength);
            if (buffer.Length >= encodedToUtf8Length)
            {
                int num1 = dataLength - dataLength / 3 * 3;
                int elementOffset1 = encodedToUtf8Length - 4;
                int elementOffset2 = dataLength - num1;
                ref byte local1 = ref s_encodingMap[0];
                ref byte local2 = ref MemoryMarshal.GetReference(buffer);
                switch (num1)
                {
                    case 0:
                        for (int elementOffset3 = elementOffset2 - 3; elementOffset3 >= 0; elementOffset3 -= 3)
                        {
                            int num2 = Encode(ref Unsafe.Add(ref local2, elementOffset3), ref local1);
                            Unsafe.WriteUnaligned<int>(ref Unsafe.Add(ref local2, elementOffset1), num2);
                            elementOffset1 -= 4;
                        }
                        bytesWritten = encodedToUtf8Length;
                        return OperationStatus.Done;
                    case 1:
                        int num3 = EncodeAndPadTwo(ref Unsafe.Add(ref local2, elementOffset2), ref local1);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref local2, elementOffset1), num3);
                        elementOffset1 -= 4;
                        goto case 0;
                    default:
                        int num4 = EncodeAndPadOne(ref Unsafe.Add(ref local2, elementOffset2), ref local1);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref local2, elementOffset1), num4);
                        elementOffset1 -= 4;
                        goto case 0;
                }
            }
            else
            {
                bytesWritten = 0;
                return OperationStatus.DestinationTooSmall;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Encode(ref byte threeBytes, ref byte encodingMap)
        {
            int num = threeBytes << 16 | Unsafe.Add(ref threeBytes, 1) << 8 | Unsafe.Add(ref threeBytes, 2);
            return Unsafe.Add(ref encodingMap, num >> 18) | Unsafe.Add(ref encodingMap, num >> 12 & 63) << 8 | Unsafe.Add(ref encodingMap, num >> 6 & 63) << 16 | Unsafe.Add(ref encodingMap, num & 63) << 24;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EncodeAndPadOne(ref byte twoBytes, ref byte encodingMap)
        {
            int num = twoBytes << 16 | Unsafe.Add(ref twoBytes, 1) << 8;
            return Unsafe.Add(ref encodingMap, num >> 18) | Unsafe.Add(ref encodingMap, num >> 12 & 63) << 8 | Unsafe.Add(ref encodingMap, num >> 6 & 63) << 16 | 1023410176;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EncodeAndPadTwo(ref byte oneByte, ref byte encodingMap)
        {
            int num = oneByte << 8;
            return Unsafe.Add(ref encodingMap, num >> 10) | Unsafe.Add(ref encodingMap, num >> 4 & 63) << 8 | 3997696 | 1023410176;
        }
    }
}
