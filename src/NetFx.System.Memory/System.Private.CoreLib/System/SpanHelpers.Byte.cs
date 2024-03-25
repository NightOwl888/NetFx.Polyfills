using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    internal static partial class SpanHelpers
    {
        public static int IndexOf(
            ref byte searchSpace,
            int searchSpaceLength,
            ref byte value,
            int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;
            byte num1 = value;
            ref byte local = ref Unsafe.Add(ref value, 1);
            int length1 = valueLength - 1;
            int elementOffset = 0;
            int num2;
            while (true)
            {
                int length2 = searchSpaceLength - elementOffset - length1;
                if (length2 > 0)
                {
                    int num3 = IndexOf(ref Unsafe.Add(ref searchSpace, elementOffset), num1, length2);
                    if (num3 != -1)
                    {
                        num2 = elementOffset + num3;
                        if (!SequenceEqual<byte>(ref Unsafe.Add(ref searchSpace, num2 + 1), ref local, length1))
                            elementOffset = num2 + 1;
                        else
                            break;
                    }
                    else
                        goto label_8;
                }
                else
                    goto label_8;
            }
            return num2;
        label_8:
            return -1;
        }

        public static unsafe int IndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = (nuint)(uint)length;

        //SequentialScan:
            while (lengthToExamine >= 8)
            {
                lengthToExamine -= 8;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;

                offset += 8;
            }

            if (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;

                offset += 1;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int) offset;
        Found1:
            return (int) (offset + 1);
        Found2:
            return (int) (offset + 2);
        Found3:
            return (int) (offset + 3);
        Found4:
            return (int) (offset + 4);
        Found5:
            return (int) (offset + 5);
        Found6:
            return (int) (offset + 6);
        Found7:
            return (int) (offset + 7);
        }

        //public static unsafe int IndexOf(ref byte searchSpace, byte value, int length)
        //{
        //    uint num1 = (uint)value;
        //    IntPtr byteOffset = (IntPtr)0;
        //    IntPtr num2 = (IntPtr)length;
        //    while ((UIntPtr)(void*)num2 >= new UIntPtr(8))
        //    {
        //        num2 -= 8;
        //        if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //        {
        //            if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
        //            {
        //                if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
        //                {
        //                    if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
        //                    {
        //                        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4))
        //                            return (int)(void*)(byteOffset + 4);
        //                        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5))
        //                            return (int)(void*)(byteOffset + 5);
        //                        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6))
        //                            return (int)(void*)(byteOffset + 6);
        //                        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7))
        //                            return (int)(void*)(byteOffset + 7);
        //                        byteOffset += 8;
        //                    }
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    if ((UIntPtr)(void*)num2 >= new UIntPtr(4))
        //    {
        //        num2 -= 4;
        //        if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //        {
        //            if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
        //            {
        //                if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
        //                {
        //                    if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
        //                        byteOffset += 4;
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    while ((UIntPtr)(void*)num2 > UIntPtr.Zero)
        //    {
        //        num2 -= 1;
        //        if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //            byteOffset += 1;
        //        else
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return (int)(void*)byteOffset;
        //label_22:
        //    return (int)(void*)(byteOffset + 1);
        //label_23:
        //    return (int)(void*)(byteOffset + 2);
        //label_24:
        //    return (int)(void*)(byteOffset + 3);
        //}


        public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

            byte valueHead = value;
            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int offset = 0;
            while (true)
            {
                Debug.Assert(0 <= offset && offset <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - offset - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                //if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, (nuint)(uint)valueTailLength))  // The (nunit)-cast is necessary to pick the correct overload
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                    return relativeIndex;  // The tail matched. Return a successful find.

                offset += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        public static int LastIndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            nuint offset = (nuint)(uint)length; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = (nuint)(uint)length;

        //SequentialScan:
            while (lengthToExamine >= 8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 7))
                    goto Found7;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 6))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 5))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 4))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            if (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 3))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 2))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset + 1))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, offset))
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int)offset;
        Found1:
            return (int)(offset + 1);
        Found2:
            return (int)(offset + 2);
        Found3:
            return (int)(offset + 3);
        Found4:
            return (int)(offset + 4);
        Found5:
            return (int)(offset + 5);
        Found6:
            return (int)(offset + 6);
        Found7:
            return (int)(offset + 7);
        }


        //      public static int LastIndexOf(
        //ref byte searchSpace,
        //int searchSpaceLength,
        //ref byte value,
        //int valueLength)
        //      {
        //          if (valueLength == 0)
        //              return 0;
        //          byte num1 = value;
        //          ref byte local = ref Unsafe.Add<byte>(ref value, 1);
        //          int length1 = valueLength - 1;
        //          int num2 = 0;
        //          int num3;
        //          while (true)
        //          {
        //              int length2 = searchSpaceLength - num2 - length1;
        //              if (length2 > 0)
        //              {
        //                  num3 = SpanHelpers.LastIndexOf(ref searchSpace, num1, length2);
        //                  if (num3 != -1)
        //                  {
        //                      if (!SpanHelpers.SequenceEqual<byte>(ref Unsafe.Add<byte>(ref searchSpace, num3 + 1), ref local, length1))
        //                          num2 += length2 - num3;
        //                      else
        //                          break;
        //                  }
        //                  else
        //                      goto label_8;
        //              }
        //              else
        //                  goto label_8;
        //          }
        //          return num3;
        //      label_8:
        //          return -1;
        //      }

        //public static unsafe int LastIndexOf(ref byte searchSpace, byte value, int length)
        //{
        //    uint num1 = (uint)value;
        //    IntPtr byteOffset = (IntPtr)length;
        //    IntPtr num2 = (IntPtr)length;
        //    while ((UIntPtr)(void*)num2 >= new UIntPtr(8))
        //    {
        //        num2 -= 8;
        //        byteOffset -= 8;
        //        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7))
        //            return (int)(void*)(byteOffset + 7);
        //        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6))
        //            return (int)(void*)(byteOffset + 6);
        //        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5))
        //            return (int)(void*)(byteOffset + 5);
        //        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4))
        //            return (int)(void*)(byteOffset + 4);
        //        if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
        //        {
        //            if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
        //            {
        //                if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
        //                {
        //                    if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //                        goto label_18;
        //                }
        //                else
        //                    goto label_19;
        //            }
        //            else
        //                goto label_20;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    if ((UIntPtr)(void*)num2 >= new UIntPtr(4))
        //    {
        //        num2 -= 4;
        //        byteOffset -= 4;
        //        if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3))
        //        {
        //            if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2))
        //            {
        //                if ((int)num1 != (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1))
        //                {
        //                    if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //                        goto label_18;
        //                }
        //                else
        //                    goto label_19;
        //            }
        //            else
        //                goto label_20;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    while ((UIntPtr)(void*)num2 > UIntPtr.Zero)
        //    {
        //        num2 -= 1;
        //        byteOffset -= 1;
        //        if ((int)num1 == (int)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset))
        //            goto label_18;
        //    }
        //    return -1;
        //label_18:
        //    return (int)(void*)byteOffset;
        //label_19:
        //    return (int)(void*)(byteOffset + 1);
        //label_20:
        //    return (int)(void*)(byteOffset + 2);
        //label_21:
        //    return (int)(void*)(byteOffset + 3);
        //}


        public static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = (nuint)(uint)length;

            uint lookUp;
            while (lengthToExamine >= 8)
            {
                lengthToExamine -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;

                offset += 8;
            }

            if (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;

                offset += 4;
            }

            while (lengthToExamine > 0)
            {

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;

                offset += 1;
                lengthToExamine -= 1;
            }

        //NotFound:
            return -1;
        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int)offset;
        Found1:
            return (int)(offset + 1);
        Found2:
            return (int)(offset + 2);
        Found3:
            return (int)(offset + 3);
        Found4:
            return (int)(offset + 4);
        Found5:
            return (int)(offset + 5);
        Found6:
            return (int)(offset + 6);
        Found7:
            return (int)(offset + 7);
        }

        //public static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        //{
        //    Debug.Assert(length >= 0);

        //    uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
        //    uint uValue1 = value1;
        //    uint uValue2 = value2;
        //    nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
        //    nuint lengthToExamine = (nuint)(uint)length;

        ////SequentialScan:
        //    uint lookUp;
        //    while (lengthToExamine >= 8)
        //    {
        //        lengthToExamine -= 8;

        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found1;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found2;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found3;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found4;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found5;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found6;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found7;

        //        offset += 8;
        //    }

        //    if (lengthToExamine >= 4)
        //    {
        //        lengthToExamine -= 4;

        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found1;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found2;
        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found3;

        //        offset += 4;
        //    }

        //    while (lengthToExamine > 0)
        //    {
        //        lengthToExamine -= 1;

        //        lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
        //        if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
        //            goto Found;

        //        offset += 1;
        //    }

        //    return -1;
        //Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        //    return (int)offset;
        //Found1:
        //    return (int)(offset + 1);
        //Found2:
        //    return (int)(offset + 2);
        //Found3:
        //    return (int)(offset + 3);
        //Found4:
        //    return (int)(offset + 4);
        //Found5:
        //    return (int)(offset + 5);
        //Found6:
        //    return (int)(offset + 6);
        //Found7:
        //    return (int)(offset + 7);
        //}


        //public static unsafe int IndexOfAny(
        //  ref byte searchSpace,
        //  byte value0,
        //  byte value1,
        //  int length)
        //{
        //    uint num1 = (uint)value0;
        //    uint num2 = (uint)value1;
        //    IntPtr byteOffset = (IntPtr)0;
        //    IntPtr num3 = (IntPtr)length;
        //    while ((UIntPtr)(void*)num3 >= new UIntPtr(8))
        //    {
        //        num3 -= 8;
        //        uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num4 && (int)num2 != (int)num4)
        //        {
        //            uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //            if ((int)num1 != (int)num5 && (int)num2 != (int)num5)
        //            {
        //                uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                if ((int)num1 != (int)num6 && (int)num2 != (int)num6)
        //                {
        //                    uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //                    if ((int)num1 != (int)num7 && (int)num2 != (int)num7)
        //                    {
        //                        uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
        //                        if ((int)num1 == (int)num8 || (int)num2 == (int)num8)
        //                            return (int)(void*)(byteOffset + 4);
        //                        uint num9 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
        //                        if ((int)num1 == (int)num9 || (int)num2 == (int)num9)
        //                            return (int)(void*)(byteOffset + 5);
        //                        uint num10 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
        //                        if ((int)num1 == (int)num10 || (int)num2 == (int)num10)
        //                            return (int)(void*)(byteOffset + 6);
        //                        uint num11 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
        //                        if ((int)num1 == (int)num11 || (int)num2 == (int)num11)
        //                            return (int)(void*)(byteOffset + 7);
        //                        byteOffset += 8;
        //                    }
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    if ((UIntPtr)(void*)num3 >= new UIntPtr(4))
        //    {
        //        num3 -= 4;
        //        uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num4 && (int)num2 != (int)num4)
        //        {
        //            uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //            if ((int)num1 != (int)num5 && (int)num2 != (int)num5)
        //            {
        //                uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                if ((int)num1 != (int)num6 && (int)num2 != (int)num6)
        //                {
        //                    uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //                    if ((int)num1 != (int)num7 && (int)num2 != (int)num7)
        //                        byteOffset += 4;
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    while ((UIntPtr)(void*)num3 > UIntPtr.Zero)
        //    {
        //        num3 -= 1;
        //        uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num4 && (int)num2 != (int)num4)
        //            byteOffset += 1;
        //        else
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return (int)(void*)byteOffset;
        //label_22:
        //    return (int)(void*)(byteOffset + 1);
        //label_23:
        //    return (int)(void*)(byteOffset + 2);
        //label_24:
        //    return (int)(void*)(byteOffset + 3);
        //}

        //public static unsafe int IndexOfAny(
        //  ref byte searchSpace,
        //  byte value0,
        //  byte value1,
        //  byte value2,
        //  int length)
        //{
        //    uint num1 = (uint)value0;
        //    uint num2 = (uint)value1;
        //    uint num3 = (uint)value2;
        //    IntPtr byteOffset = (IntPtr)0;
        //    IntPtr num4 = (IntPtr)length;
        //    while ((UIntPtr)(void*)num4 >= new UIntPtr(8))
        //    {
        //        num4 -= 8;
        //        uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num5 && (int)num2 != (int)num5 && (int)num3 != (int)num5)
        //        {
        //            uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //            if ((int)num1 != (int)num6 && (int)num2 != (int)num6 && (int)num3 != (int)num6)
        //            {
        //                uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                if ((int)num1 != (int)num7 && (int)num2 != (int)num7 && (int)num3 != (int)num7)
        //                {
        //                    uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //                    if ((int)num1 != (int)num8 && (int)num2 != (int)num8 && (int)num3 != (int)num8)
        //                    {
        //                        uint num9 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
        //                        if ((int)num1 == (int)num9 || (int)num2 == (int)num9 || (int)num3 == (int)num9)
        //                            return (int)(void*)(byteOffset + 4);
        //                        uint num10 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
        //                        if ((int)num1 == (int)num10 || (int)num2 == (int)num10 || (int)num3 == (int)num10)
        //                            return (int)(void*)(byteOffset + 5);
        //                        uint num11 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
        //                        if ((int)num1 == (int)num11 || (int)num2 == (int)num11 || (int)num3 == (int)num11)
        //                            return (int)(void*)(byteOffset + 6);
        //                        uint num12 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
        //                        if ((int)num1 == (int)num12 || (int)num2 == (int)num12 || (int)num3 == (int)num12)
        //                            return (int)(void*)(byteOffset + 7);
        //                        byteOffset += 8;
        //                    }
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    if ((UIntPtr)(void*)num4 >= new UIntPtr(4))
        //    {
        //        num4 -= 4;
        //        uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num5 && (int)num2 != (int)num5 && (int)num3 != (int)num5)
        //        {
        //            uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //            if ((int)num1 != (int)num6 && (int)num2 != (int)num6 && (int)num3 != (int)num6)
        //            {
        //                uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                if ((int)num1 != (int)num7 && (int)num2 != (int)num7 && (int)num3 != (int)num7)
        //                {
        //                    uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //                    if ((int)num1 != (int)num8 && (int)num2 != (int)num8 && (int)num3 != (int)num8)
        //                        byteOffset += 4;
        //                    else
        //                        goto label_24;
        //                }
        //                else
        //                    goto label_23;
        //            }
        //            else
        //                goto label_22;
        //        }
        //        else
        //            goto label_21;
        //    }
        //    while ((UIntPtr)(void*)num4 > UIntPtr.Zero)
        //    {
        //        num4 -= 1;
        //        uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //        if ((int)num1 != (int)num5 && (int)num2 != (int)num5 && (int)num3 != (int)num5)
        //            byteOffset += 1;
        //        else
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return (int)(void*)byteOffset;
        //label_22:
        //    return (int)(void*)(byteOffset + 1);
        //label_23:
        //    return (int)(void*)(byteOffset + 2);
        //label_24:
        //    return (int)(void*)(byteOffset + 3);
        //}


        //public static int IndexOfAny(
        //  ref byte searchSpace,
        //  int searchSpaceLength,
        //  ref byte value,
        //  int valueLength)
        //{
        //    if (valueLength == 0)
        //        return 0;
        //    int num1 = -1;
        //    for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
        //    {
        //        int num2 = SpanHelpers.IndexOf(ref searchSpace, Unsafe.Add<byte>(ref value, elementOffset), searchSpaceLength);
        //        if ((uint)num2 < (uint)num1)
        //        {
        //            num1 = num2;
        //            searchSpaceLength = num2;
        //            if (num1 == 0)
        //                break;
        //        }
        //    }
        //    return num1;
        //}


        public static int LastIndexOfAny(
          ref byte searchSpace,
          int searchSpaceLength,
          ref byte value,
          int valueLength)
        {
            if (valueLength == 0)
                return 0;
            int num1 = -1;
            for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
            {
                int num2 = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, elementOffset), searchSpaceLength);
                if (num2 > num1)
                    num1 = num2;
            }
            return num1;
        }

        public static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            nuint offset = (nuint)(uint)length; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = (nuint)(uint)length;

        //SequentialScan:
            uint lookUp;
            while (lengthToExamine >= 8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            if (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int)offset;
        Found1:
            return (int)(offset + 1);
        Found2:
            return (int)(offset + 2);
        Found3:
            return (int)(offset + 3);
        Found4:
            return (int)(offset + 4);
        Found5:
            return (int)(offset + 5);
        Found6:
            return (int)(offset + 6);
        Found7:
            return (int)(offset + 7);
        }

        public static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1;
            uint uValue2 = value2;
            nuint offset = (nuint)(uint)length; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = (nuint)(uint)length;

        //SequentialScan:
            uint lookUp;
            while (lengthToExamine >= 8)
            {
                lengthToExamine -= 8;
                offset -= 8;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 7);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 6);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 5);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 4);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            if (lengthToExamine >= 4)
            {
                lengthToExamine -= 4;
                offset -= 4;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 3);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 2);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset + 1);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            while (lengthToExamine > 0)
            {
                lengthToExamine -= 1;
                offset -= 1;

                lookUp = Unsafe.AddByteOffset(ref searchSpace, offset);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            return -1;
        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int)offset;
        Found1:
            return (int)(offset + 1);
        Found2:
            return (int)(offset + 2);
        Found3:
            return (int)(offset + 3);
        Found4:
            return (int)(offset + 4);
        Found5:
            return (int)(offset + 5);
        Found6:
            return (int)(offset + 6);
        Found7:
            return (int)(offset + 7);
        }



        //      public static unsafe int LastIndexOfAny(
        //ref byte searchSpace,
        //byte value0,
        //byte value1,
        //int length)
        //      {
        //          uint num1 = (uint)value0;
        //          uint num2 = (uint)value1;
        //          IntPtr byteOffset = (IntPtr)length;
        //          IntPtr num3 = (IntPtr)length;
        //          while ((UIntPtr)(void*)num3 >= new UIntPtr(8))
        //          {
        //              num3 -= 8;
        //              byteOffset -= 8;
        //              uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
        //              if ((int)num1 == (int)num4 || (int)num2 == (int)num4)
        //                  return (int)(void*)(byteOffset + 7);
        //              uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
        //              if ((int)num1 == (int)num5 || (int)num2 == (int)num5)
        //                  return (int)(void*)(byteOffset + 6);
        //              uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
        //              if ((int)num1 == (int)num6 || (int)num2 == (int)num6)
        //                  return (int)(void*)(byteOffset + 5);
        //              uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
        //              if ((int)num1 == (int)num7 || (int)num2 == (int)num7)
        //                  return (int)(void*)(byteOffset + 4);
        //              uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //              if ((int)num1 != (int)num8 && (int)num2 != (int)num8)
        //              {
        //                  uint num9 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                  if ((int)num1 != (int)num9 && (int)num2 != (int)num9)
        //                  {
        //                      uint num10 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //                      if ((int)num1 != (int)num10 && (int)num2 != (int)num10)
        //                      {
        //                          uint num11 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //                          if ((int)num1 == (int)num11 || (int)num2 == (int)num11)
        //                              goto label_18;
        //                      }
        //                      else
        //                          goto label_19;
        //                  }
        //                  else
        //                      goto label_20;
        //              }
        //              else
        //                  goto label_21;
        //          }
        //          if ((UIntPtr)(void*)num3 >= new UIntPtr(4))
        //          {
        //              num3 -= 4;
        //              byteOffset -= 4;
        //              uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //              if ((int)num1 != (int)num4 && (int)num2 != (int)num4)
        //              {
        //                  uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                  if ((int)num1 != (int)num5 && (int)num2 != (int)num5)
        //                  {
        //                      uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //                      if ((int)num1 != (int)num6 && (int)num2 != (int)num6)
        //                      {
        //                          uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //                          if ((int)num1 == (int)num7 || (int)num2 == (int)num7)
        //                              goto label_18;
        //                      }
        //                      else
        //                          goto label_19;
        //                  }
        //                  else
        //                      goto label_20;
        //              }
        //              else
        //                  goto label_21;
        //          }
        //          while ((UIntPtr)(void*)num3 > UIntPtr.Zero)
        //          {
        //              num3 -= 1;
        //              byteOffset -= 1;
        //              uint num4 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //              if ((int)num1 == (int)num4 || (int)num2 == (int)num4)
        //                  goto label_18;
        //          }
        //          return -1;
        //      label_18:
        //          return (int)(void*)byteOffset;
        //      label_19:
        //          return (int)(void*)(byteOffset + 1);
        //      label_20:
        //          return (int)(void*)(byteOffset + 2);
        //      label_21:
        //          return (int)(void*)(byteOffset + 3);
        //      }

        //      public static unsafe int LastIndexOfAny(
        //        ref byte searchSpace,
        //        byte value0,
        //        byte value1,
        //        byte value2,
        //        int length)
        //      {
        //          uint num1 = (uint)value0;
        //          uint num2 = (uint)value1;
        //          uint num3 = (uint)value2;
        //          IntPtr byteOffset = (IntPtr)length;
        //          IntPtr num4 = (IntPtr)length;
        //          while ((UIntPtr)(void*)num4 >= new UIntPtr(8))
        //          {
        //              num4 -= 8;
        //              byteOffset -= 8;
        //              uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 7);
        //              if ((int)num1 == (int)num5 || (int)num2 == (int)num5 || (int)num3 == (int)num5)
        //                  return (int)(void*)(byteOffset + 7);
        //              uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 6);
        //              if ((int)num1 == (int)num6 || (int)num2 == (int)num6 || (int)num3 == (int)num6)
        //                  return (int)(void*)(byteOffset + 6);
        //              uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 5);
        //              if ((int)num1 == (int)num7 || (int)num2 == (int)num7 || (int)num3 == (int)num7)
        //                  return (int)(void*)(byteOffset + 5);
        //              uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 4);
        //              if ((int)num1 == (int)num8 || (int)num2 == (int)num8 || (int)num3 == (int)num8)
        //                  return (int)(void*)(byteOffset + 4);
        //              uint num9 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //              if ((int)num1 != (int)num9 && (int)num2 != (int)num9 && (int)num3 != (int)num9)
        //              {
        //                  uint num10 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                  if ((int)num1 != (int)num10 && (int)num2 != (int)num10 && (int)num3 != (int)num10)
        //                  {
        //                      uint num11 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //                      if ((int)num1 != (int)num11 && (int)num2 != (int)num11 && (int)num3 != (int)num11)
        //                      {
        //                          uint num12 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //                          if ((int)num1 == (int)num12 || (int)num2 == (int)num12 || (int)num3 == (int)num12)
        //                              goto label_18;
        //                      }
        //                      else
        //                          goto label_19;
        //                  }
        //                  else
        //                      goto label_20;
        //              }
        //              else
        //                  goto label_21;
        //          }
        //          if ((UIntPtr)(void*)num4 >= new UIntPtr(4))
        //          {
        //              num4 -= 4;
        //              byteOffset -= 4;
        //              uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 3);
        //              if ((int)num1 != (int)num5 && (int)num2 != (int)num5 && (int)num3 != (int)num5)
        //              {
        //                  uint num6 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 2);
        //                  if ((int)num1 != (int)num6 && (int)num2 != (int)num6 && (int)num3 != (int)num6)
        //                  {
        //                      uint num7 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset + 1);
        //                      if ((int)num1 != (int)num7 && (int)num2 != (int)num7 && (int)num3 != (int)num7)
        //                      {
        //                          uint num8 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //                          if ((int)num1 == (int)num8 || (int)num2 == (int)num8 || (int)num3 == (int)num8)
        //                              goto label_18;
        //                      }
        //                      else
        //                          goto label_19;
        //                  }
        //                  else
        //                      goto label_20;
        //              }
        //              else
        //                  goto label_21;
        //          }
        //          while ((UIntPtr)(void*)num4 > UIntPtr.Zero)
        //          {
        //              num4 -= 1;
        //              byteOffset -= 1;
        //              uint num5 = (uint)Unsafe.AddByteOffset<byte>(ref searchSpace, byteOffset);
        //              if ((int)num1 == (int)num5 || (int)num2 == (int)num5 || (int)num3 == (int)num5)
        //                  goto label_18;
        //          }
        //          return -1;
        //      label_18:
        //          return (int)(void*)byteOffset;
        //      label_19:
        //          return (int)(void*)(byteOffset + 1);
        //      label_20:
        //          return (int)(void*)(byteOffset + 2);
        //      label_21:
        //          return (int)(void*)(byteOffset + 3);
        //      }


        // Optimized byte-based SequenceEquals. The "length" parameter for this one is declared a nuint rather than int as we also use it for types other than byte
        // where the length can exceed 2Gb once scaled by sizeof(T).
        public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length)
        {
            bool result;
            // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            if (length >= (nuint)sizeof(nuint))
            {
                // Conditional jmp foward to favor shorter lengths. (See comment at "Equal:" label)
                // The longer lengths can make back the time due to branch misprediction
                // better than shorter lengths.
                goto Longer;
            }

#if TARGET_64BIT
            // On 32-bit, this will always be true since sizeof(nuint) == 4
            if (length < sizeof(uint))
#endif
            {
                uint differentBits = 0;
                nuint offset = (length & 2);
                if (offset != 0)
                {
                    differentBits = LoadUShort(ref first);
                    differentBits -= LoadUShort(ref second);
                }
                if ((length & 1) != 0)
                {
                    differentBits |= (uint)Unsafe.AddByteOffset(ref first, offset) - (uint)Unsafe.AddByteOffset(ref second, offset);
                }
                result = (differentBits == 0);
                goto Result;
            }
#if TARGET_64BIT
            else
            {
                nuint offset = length - sizeof(uint);
                uint differentBits = LoadUInt(ref first) - LoadUInt(ref second);
                differentBits |= LoadUInt(ref first, offset) - LoadUInt(ref second, offset);
                result = (differentBits == 0);
                goto Result;
            }
#endif
        Longer:
            // Only check that the ref is the same if buffers are large,
            // and hence its worth avoiding doing unnecessary comparisons
            if (!Unsafe.AreSame(ref first, ref second))
            {
                // C# compiler inverts this test, making the outer goto the conditional jmp.
                goto Vector;
            }

            // This becomes a conditional jmp foward to not favor it.
            goto Equal;

        Result:
            return result;
        // When the sequence is equal; which is the longest execution, we want it to determine that
        // as fast as possible so we do not want the early outs to be "predicted not taken" branches.
        Equal:
            return true;

        Vector:
            {
                Debug.Assert(length >= (nuint)sizeof(nuint));
                {
                    nuint offset = 0;
                    nuint lengthToExamine = length - (nuint)sizeof(nuint);
                    // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                    Debug.Assert(lengthToExamine < length);
                    if (lengthToExamine > 0)
                    {
                        do
                        {
                            // Compare unsigned so not do a sign extend mov on 64 bit
                            if (LoadNUInt(ref first, offset) != LoadNUInt(ref second, offset))
                            {
                                goto NotEqual;
                            }
                            offset += (nuint)sizeof(nuint);
                        } while (lengthToExamine > offset);
                    }

                    // Do final compare as sizeof(nuint) from end rather than start
                    result = (LoadNUInt(ref first, lengthToExamine) == LoadNUInt(ref second, lengthToExamine));
                    goto Result;
                }
            }

        // As there are so many true/false exit points the Jit will coalesce them to one location.
        // We want them at the end so the conditional early exit jmps are all jmp forwards so the
        // branch predictor in a uninitialized state will not take them e.g.
        // - loops are conditional jmps backwards and predicted
        // - exceptions are conditional fowards jmps and not predicted
        NotEqual:
            return false;
        }



        //public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length)
        //{
        //    if (!Unsafe.AreSame(ref first, ref second))
        //    {
        //        IntPtr byteOffset1 = (IntPtr)0;
        //        IntPtr num = (IntPtr)(void*)length;
        //        // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
        //        if (length >= (nuint)sizeof(nuint))
        //        {
        //            IntPtr byteOffset2 = num - sizeof(UIntPtr);
        //            while ((void*)byteOffset2 > (void*)byteOffset1)
        //            {
        //                if (!(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref first, byteOffset1)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref second, byteOffset1))))
        //                    byteOffset1 += sizeof(UIntPtr);
        //                else
        //                    goto NotEqual;
        //            }
        //            return Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref first, byteOffset2)) == Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref second, byteOffset2));
        //        }
        //        while ((void*)num > (void*)byteOffset1)
        //        {
        //            if ((int)Unsafe.AddByteOffset<byte>(ref first, byteOffset1) == (int)Unsafe.AddByteOffset<byte>(ref second, byteOffset1))
        //                byteOffset1 += 1;
        //            else
        //                goto NotEqual;
        //        }
        //        goto Equal;
        //    NotEqual:
        //        // As there are so many true/false exit points the Jit will coalesce them to one location.
        //        // We want them at the end so the conditional early exit jmps are all jmp forwards so the
        //        // branch predictor in a uninitialized state will not take them e.g.
        //        // - loops are conditional jmps backwards and predicted
        //        // - exceptions are conditional fowards jmps and not predicted
        //        return false;
        //    }
        //Equal:
        //    return true;
        //}

        public static unsafe int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength)
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            nuint minLength = (nuint)(((uint)firstLength < (uint)secondLength) ? (uint)firstLength : (uint)secondLength);

            nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
            nuint lengthToExamine = minLength;

            if (lengthToExamine > (nuint)sizeof(nuint))
            {
                lengthToExamine -= (nuint)sizeof(nuint);
                while (lengthToExamine > offset)
                {
                    if (LoadNUInt(ref first, offset) != LoadNUInt(ref second, offset))
                    {
                        goto BytewiseCheck;
                    }
                    offset += (nuint)sizeof(nuint);
                }
            }

        BytewiseCheck:  // Workaround for https://github.com/dotnet/runtime/issues/8795
            while (minLength > offset)
            {
                int result = Unsafe.AddByteOffset(ref first, offset).CompareTo(Unsafe.AddByteOffset(ref second, offset));
                if (result != 0)
                    return result;
                offset += 1;
            }

        Equal:
            return firstLength - secondLength;
        }

        //public static unsafe int SequenceCompareTo(
        //    ref byte first,
        //    int firstLength,
        //    ref byte second,
        //    int secondLength)
        //{
        //    if (!Unsafe.AreSame<byte>(ref first, ref second))
        //    {
        //        IntPtr num1 = (IntPtr)(firstLength < secondLength ? firstLength : secondLength);
        //        IntPtr byteOffset = (IntPtr)0;
        //        IntPtr num2 = (IntPtr)(void*)num1;
        //        if ((UIntPtr)(void*)num2 > (UIntPtr)sizeof(UIntPtr))
        //        {
        //            IntPtr num3 = num2 - sizeof(UIntPtr);
        //            while ((void*)num3 > (void*)byteOffset && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref first, byteOffset)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset<byte>(ref second, byteOffset))))
        //                byteOffset += sizeof(UIntPtr);
        //        }
        //        while ((void*)num1 > (void*)byteOffset)
        //        {
        //            int num3 = Unsafe.AddByteOffset<byte>(ref first, byteOffset).CompareTo(Unsafe.AddByteOffset<byte>(ref second, byteOffset));
        //            if (num3 != 0)
        //                return num3;
        //            byteOffset += 1;
        //        }
        //    }
        //    return firstLength - secondLength;
        //}


        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static int LocateFirstFoundByte(ulong match)
        //    => BitOperations.TrailingZeroCount(match) >> 3;

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static int LocateLastFoundByte(ulong match)
        //    => BitOperations.Log2(match) >> 3;

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort LoadUShort(ref byte start)
            => Unsafe.ReadUnaligned<ushort>(ref start);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LoadUInt(ref byte start)
            => Unsafe.ReadUnaligned<uint>(ref start);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LoadUInt(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref start, offset));

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint LoadNUInt(ref byte start)
            => Unsafe.ReadUnaligned<nuint>(ref start);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint LoadNUInt(ref byte start, nuint offset)
            => Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref start, offset));
    }
}
