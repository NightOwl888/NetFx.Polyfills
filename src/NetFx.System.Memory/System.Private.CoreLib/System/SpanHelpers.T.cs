﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static partial class SpanHelpers
    {
        public static void Fill<T>(ref T refData, nuint numElements, T value)
        {
            nuint i = 0;

            // Write 8 elements at a time

            if (numElements >= 8)
            {
                nuint stopLoopAtOffset = numElements & ~(nuint)7;
                do
                {
                    Unsafe.Add(ref refData, (nint)i + 0) = value;
                    Unsafe.Add(ref refData, (nint)i + 1) = value;
                    Unsafe.Add(ref refData, (nint)i + 2) = value;
                    Unsafe.Add(ref refData, (nint)i + 3) = value;
                    Unsafe.Add(ref refData, (nint)i + 4) = value;
                    Unsafe.Add(ref refData, (nint)i + 5) = value;
                    Unsafe.Add(ref refData, (nint)i + 6) = value;
                    Unsafe.Add(ref refData, (nint)i + 7) = value;
                } while ((i += 8) < stopLoopAtOffset);
            }

            // Write next 4 elements if needed

            if ((numElements & 4) != 0)
            {
                Unsafe.Add(ref refData, (nint)i + 0) = value;
                Unsafe.Add(ref refData, (nint)i + 1) = value;
                Unsafe.Add(ref refData, (nint)i + 2) = value;
                Unsafe.Add(ref refData, (nint)i + 3) = value;
                i += 4;
            }

            // Write next 2 elements if needed

            if ((numElements & 2) != 0)
            {
                Unsafe.Add(ref refData, (nint)i + 0) = value;
                Unsafe.Add(ref refData, (nint)i + 1) = value;
                i += 2;
            }

            // Write final element if needed

            if ((numElements & 1) != 0)
            {
                Unsafe.Add(ref refData, (nint)i) = value;
            }
        }

        public static int IndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            T valueHead = value;
            ref T valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int index = 0;
            while (true)
            {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;
                index += relativeIndex;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, index + 1), ref valueTail, valueTailLength))
                    return index;  // The tail matched. Return a successful find.

                index++;
            }
            return -1;
        }

        //public static int IndexOf<T>(
        //  ref T searchSpace,
        //  int searchSpaceLength,
        //  ref T value,
        //  int valueLength)
        //  where T : IEquatable<T>
        //{
        //    if (valueLength == 0)
        //        return 0;
        //    T obj = value;
        //    ref T local = ref Unsafe.Add<T>(ref value, 1);
        //    int length1 = valueLength - 1;
        //    int elementOffset = 0;
        //    int num1;
        //    while (true)
        //    {
        //        int length2 = searchSpaceLength - elementOffset - length1;
        //        if (length2 > 0)
        //        {
        //            int num2 = SpanHelpers.IndexOf<T>(ref Unsafe.Add<T>(ref searchSpace, elementOffset), obj, length2);
        //            if (num2 != -1)
        //            {
        //                num1 = elementOffset + num2;
        //                if (!SpanHelpers.SequenceEqual<T>(ref Unsafe.Add<T>(ref searchSpace, num1 + 1), ref local, length1))
        //                    elementOffset = num1 + 1;
        //                else
        //                    break;
        //            }
        //            else
        //                goto label_8;
        //        }
        //        else
        //            goto label_8;
        //    }
        //    return num1;
        //label_8:
        //    return -1;
        //}

        public static unsafe int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            if (default(T) != null || (object)value != null)
            {
                while (length >= 8)
                {
                    length -= 8;

                    if (value.Equals(Unsafe.Add(ref searchSpace, index)))
                        goto Found;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 1)))
                        goto Found1;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 2)))
                        goto Found2;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                        goto Found3;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 4)))
                        goto Found4;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 5)))
                        goto Found5;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 6)))
                        goto Found6;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 7)))
                        goto Found7;

                    index += 8;
                }

                if (length >= 4)
                {
                    length -= 4;

                    if (value.Equals(Unsafe.Add(ref searchSpace, index)))
                        goto Found;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 1)))
                        goto Found1;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 2)))
                        goto Found2;
                    if (value.Equals(Unsafe.Add(ref searchSpace, index + 3)))
                        goto Found3;

                    index += 4;
                }

                while (length > 0)
                {
                    if (value.Equals(Unsafe.Add(ref searchSpace, index)))
                        goto Found;

                    index += 1;
                    length--;
                }
            }
            else
            {
                nint len = (nint)length;
                for (index = 0; index < len; index++)
                {
                    if ((object)Unsafe.Add(ref searchSpace, index) is null)
                    {
                        goto Found;
                    }
                }
            }
            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return (int)index;
        Found1:
            return (int)(index + 1);
        Found2:
            return (int)(index + 2);
        Found3:
            return (int)(index + 3);
        Found4:
            return (int)(index + 4);
        Found5:
            return (int)(index + 5);
        Found6:
            return (int)(index + 6);
        Found7:
            return (int)(index + 7);
        }

        //public static unsafe int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        //{
        //    IntPtr elementOffset = (IntPtr)0;
        //    while (length >= 8)
        //    {
        //        length -= 8;
        //        if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
        //        {
        //            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 1)))
        //            {
        //                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 2)))
        //                {
        //                    if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 3)))
        //                    {
        //                        if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 4)))
        //                            return (int)(void*)(elementOffset + 4);
        //                        if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 5)))
        //                            return (int)(void*)(elementOffset + 5);
        //                        if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 6)))
        //                            return (int)(void*)(elementOffset + 6);
        //                        if (value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 7)))
        //                            return (int)(void*)(elementOffset + 7);
        //                        elementOffset += 8;
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
        //    if (length >= 4)
        //    {
        //        length -= 4;
        //        if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
        //        {
        //            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 1)))
        //            {
        //                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 2)))
        //                {
        //                    if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset + 3)))
        //                        elementOffset += 4;
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
        //    for (; length > 0; --length)
        //    {
        //        if (!value.Equals(Unsafe.Add<T>(ref searchSpace, elementOffset)))
        //            elementOffset += 1;
        //        else
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return (int)(void*)elementOffset;
        //label_22:
        //    return (int)(void*)(elementOffset + 1);
        //label_23:
        //    return (int)(void*)(elementOffset + 2);
        //label_24:
        //    return (int)(void*)(elementOffset + 3);
        //}


        public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            T lookUp;
            int index = 0;
            if (default(T) != null || ((object)value0 != null && (object)value1 != null))
            {
                while ((length - index) >= 8)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                    lookUp = Unsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, index + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found4;
                    lookUp = Unsafe.Add(ref searchSpace, index + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found5;
                    lookUp = Unsafe.Add(ref searchSpace, index + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found6;
                    lookUp = Unsafe.Add(ref searchSpace, index + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found7;

                    index += 8;
                }

                if ((length - index) >= 4)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                    lookUp = Unsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;

                    index += 4;
                }

                while (index < length)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;

                    index++;
                }
            }
            else
            {
                for (index = 0; index < length; index++)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if ((object?)lookUp is null)
                    {
                        if ((object?)value0 is null || (object?)value1 is null)
                        {
                            goto Found;
                        }
                    }
                    else if (lookUp.Equals(value0!) || lookUp.Equals(value1!))
                    {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return index;
        Found1:
            return index + 1;
        Found2:
            return index + 2;
        Found3:
            return index + 3;
        Found4:
            return index + 4;
        Found5:
            return index + 5;
        Found6:
            return index + 6;
        Found7:
            return index + 7;
        }

        //public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        //{
        //    int elementOffset;
        //    for (elementOffset = 0; length - elementOffset >= 8; elementOffset += 8)
        //    {
        //        T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (!value0.Equals(other1) && !value1.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
        //            if (!value0.Equals(other2) && !value1.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
        //                if (!value0.Equals(other3) && !value1.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
        //                    if (!value0.Equals(other4) && !value1.Equals(other4))
        //                    {
        //                        T other5 = Unsafe.Add<T>(ref searchSpace, elementOffset + 4);
        //                        if (value0.Equals(other5) || value1.Equals(other5))
        //                            return elementOffset + 4;
        //                        T other6 = Unsafe.Add<T>(ref searchSpace, elementOffset + 5);
        //                        if (value0.Equals(other6) || value1.Equals(other6))
        //                            return elementOffset + 5;
        //                        T other7 = Unsafe.Add<T>(ref searchSpace, elementOffset + 6);
        //                        if (value0.Equals(other7) || value1.Equals(other7))
        //                            return elementOffset + 6;
        //                        T other8 = Unsafe.Add<T>(ref searchSpace, elementOffset + 7);
        //                        if (value0.Equals(other8) || value1.Equals(other8))
        //                            return elementOffset + 7;
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
        //    if (length - elementOffset >= 4)
        //    {
        //        T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (!value0.Equals(other1) && !value1.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
        //            if (!value0.Equals(other2) && !value1.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
        //                if (!value0.Equals(other3) && !value1.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
        //                    if (!value0.Equals(other4) && !value1.Equals(other4))
        //                        elementOffset += 4;
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
        //    for (; elementOffset < length; ++elementOffset)
        //    {
        //        T other = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (value0.Equals(other) || value1.Equals(other))
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return elementOffset;
        //label_22:
        //    return elementOffset + 1;
        //label_23:
        //    return elementOffset + 2;
        //label_24:
        //    return elementOffset + 3;
        //}


        public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            T lookUp;
            int index = 0;
            if (default(T) != null || ((object)value0 != null && (object)value1 != null && (object)value2 != null))
            {
                while ((length - index) >= 8)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                    lookUp = Unsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, index + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found4;
                    lookUp = Unsafe.Add(ref searchSpace, index + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found5;
                    lookUp = Unsafe.Add(ref searchSpace, index + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found6;
                    lookUp = Unsafe.Add(ref searchSpace, index + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found7;

                    index += 8;
                }

                if ((length - index) >= 4)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                    lookUp = Unsafe.Add(ref searchSpace, index + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, index + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, index + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;

                    index += 4;
                }

                while (index < length)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;

                    index++;
                }
            }
            else
            {
                for (index = 0; index < length; index++)
                {
                    lookUp = Unsafe.Add(ref searchSpace, index);
                    if ((object?)lookUp is null)
                    {
                        if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null)
                        {
                            goto Found;
                        }
                    }
                    else if (lookUp.Equals(value0!) || lookUp.Equals(value1!) || lookUp.Equals(value2!))
                    {
                        goto Found;
                    }
                }
            }
            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return index;
        Found1:
            return index + 1;
        Found2:
            return index + 2;
        Found3:
            return index + 3;
        Found4:
            return index + 4;
        Found5:
            return index + 5;
        Found6:
            return index + 6;
        Found7:
            return index + 7;
        }

        //public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
        //{
        //    int elementOffset;
        //    for (elementOffset = 0; length - elementOffset >= 8; elementOffset += 8)
        //    {
        //        T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
        //            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
        //                if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
        //                    if (!value0.Equals(other4) && !value1.Equals(other4) && !value2.Equals(other4))
        //                    {
        //                        T other5 = Unsafe.Add<T>(ref searchSpace, elementOffset + 4);
        //                        if (value0.Equals(other5) || value1.Equals(other5) || value2.Equals(other5))
        //                            return elementOffset + 4;
        //                        T other6 = Unsafe.Add<T>(ref searchSpace, elementOffset + 5);
        //                        if (value0.Equals(other6) || value1.Equals(other6) || value2.Equals(other6))
        //                            return elementOffset + 5;
        //                        T other7 = Unsafe.Add<T>(ref searchSpace, elementOffset + 6);
        //                        if (value0.Equals(other7) || value1.Equals(other7) || value2.Equals(other7))
        //                            return elementOffset + 6;
        //                        T other8 = Unsafe.Add<T>(ref searchSpace, elementOffset + 7);
        //                        if (value0.Equals(other8) || value1.Equals(other8) || value2.Equals(other8))
        //                            return elementOffset + 7;
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
        //    if (length - elementOffset >= 4)
        //    {
        //        T other1 = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, elementOffset + 1);
        //            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, elementOffset + 2);
        //                if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, elementOffset + 3);
        //                    if (!value0.Equals(other4) && !value1.Equals(other4) && !value2.Equals(other4))
        //                        elementOffset += 4;
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
        //    for (; elementOffset < length; ++elementOffset)
        //    {
        //        T other = Unsafe.Add<T>(ref searchSpace, elementOffset);
        //        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
        //            goto label_21;
        //    }
        //    return -1;
        //label_21:
        //    return elementOffset;
        //label_22:
        //    return elementOffset + 1;
        //label_23:
        //    return elementOffset + 2;
        //label_24:
        //    return elementOffset + 3;
        //}

        public static int IndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return -1;  // A zero-length set of values is always treated as "not found".

            // For the following paragraph, let:
            //   n := length of haystack
            //   i := index of first occurrence of any needle within haystack
            //   l := length of needle array
            //
            // We use a naive non-vectorized search because we want to bound the complexity of IndexOfAny
            // to O(i * l) rather than O(n * l), or just O(n * l) if no needle is found. The reason for
            // this is that it's common for callers to invoke IndexOfAny immediately before slicing,
            // and when this is called in a loop, we want the entire loop to be bounded by O(n * l)
            // rather than O(n^2 * l).

            if (typeof(T).IsValueType)
            {
                // Calling ValueType.Equals (devirtualized), which takes 'this' byref. We'll make
                // a byval copy of the candidate from the search space in the outer loop, then in
                // the inner loop we'll pass a ref (as 'this') to each element in the needle.

                for (int i = 0; i < searchSpaceLength; i++)
                {
                    T candidate = Unsafe.Add(ref searchSpace, i);
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (Unsafe.Add(ref value, j).Equals(candidate))
                        {
                            return i;
                        }
                    }
                }
            }
            else
            {
                // Calling IEquatable<T>.Equals (virtual dispatch). We'll perform the null check
                // in the outer loop instead of in the inner loop to save some branching.

                for (int i = 0; i < searchSpaceLength; i++)
                {
                    T candidate = Unsafe.Add(ref searchSpace, i);
                    if (candidate is not null)
                    {
                        for (int j = 0; j < valueLength; j++)
                        {
                            if (candidate.Equals(Unsafe.Add(ref value, j)))
                            {
                                return i;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < valueLength; j++)
                        {
                            if (Unsafe.Add(ref value, j) is null)
                            {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1; // not found
        }

        //public static int IndexOfAny<T>(
        //  ref T searchSpace,
        //  int searchSpaceLength,
        //  ref T value,
        //  int valueLength)
        //  where T : IEquatable<T>
        //{
        //    if (valueLength == 0)
        //        return 0;
        //    int num1 = -1;
        //    for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
        //    {
        //        int num2 = SpanHelpers.IndexOf<T>(ref searchSpace, Unsafe.Add<T>(ref value, elementOffset), searchSpaceLength);
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


        public static int LastIndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return searchSpaceLength;  // A zero-length sequence is always treated as "found" at the end of the search space.

            T valueHead = value;
            ref T valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int index = 0;
            while (true)
            {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                    return relativeIndex;  // The tail matched. Return a successful find.

                index += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }


        //public static int LastIndexOf<T>(
        //  ref T searchSpace,
        //  int searchSpaceLength,
        //  ref T value,
        //  int valueLength)
        //  where T : IEquatable<T>
        //{
        //    if (valueLength == 0)
        //        return 0;
        //    T obj = value;
        //    ref T local = ref Unsafe.Add<T>(ref value, 1);
        //    int length1 = valueLength - 1;
        //    int num1 = 0;
        //    int num2;
        //    while (true)
        //    {
        //        int length2 = searchSpaceLength - num1 - length1;
        //        if (length2 > 0)
        //        {
        //            num2 = SpanHelpers.LastIndexOf<T>(ref searchSpace, obj, length2);
        //            if (num2 != -1)
        //            {
        //                if (!SpanHelpers.SequenceEqual<T>(ref Unsafe.Add<T>(ref searchSpace, num2 + 1), ref local, length1))
        //                    num1 += length2 - num2;
        //                else
        //                    break;
        //            }
        //            else
        //                goto label_8;
        //        }
        //        else
        //            goto label_8;
        //    }
        //    return num2;
        //label_8:
        //    return -1;
        //}


        public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            if (default(T) != null || (object)value != null)
            {
                while (length >= 8)
                {
                    length -= 8;

                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 7)))
                        goto Found7;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 6)))
                        goto Found6;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 5)))
                        goto Found5;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 4)))
                        goto Found4;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                        goto Found3;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                        goto Found2;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                        goto Found1;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                        goto Found;
                }

                if (length >= 4)
                {
                    length -= 4;

                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                        goto Found3;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                        goto Found2;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                        goto Found1;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                        goto Found;
                }

                while (length > 0)
                {
                    length--;

                    if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                        goto Found;
                }
            }
            else
            {
                for (length--; length >= 0; length--)
                {
                    if ((object)Unsafe.Add(ref searchSpace, length) is null)
                    {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }



        //public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        //{
        //    while (length >= 8)
        //    {
        //        length -= 8;
        //        if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 7)))
        //            return length + 7;
        //        if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 6)))
        //            return length + 6;
        //        if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 5)))
        //            return length + 5;
        //        if (value.Equals(Unsafe.Add<T>(ref searchSpace, length + 4)))
        //            return length + 4;
        //        if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 3)))
        //        {
        //            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 2)))
        //            {
        //                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 1)))
        //                {
        //                    if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
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
        //    if (length >= 4)
        //    {
        //        length -= 4;
        //        if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 3)))
        //        {
        //            if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 2)))
        //            {
        //                if (!value.Equals(Unsafe.Add<T>(ref searchSpace, length + 1)))
        //                {
        //                    if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
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
        //    while (length > 0)
        //    {
        //        --length;
        //        if (value.Equals(Unsafe.Add<T>(ref searchSpace, length)))
        //            goto label_18;
        //    }
        //    return -1;
        //label_18:
        //    return length;
        //label_19:
        //    return length + 1;
        //label_20:
        //    return length + 2;
        //label_21:
        //    return length + 3;
        //}

        public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            T lookUp;
            if (default(T) != null || ((object)value0 != null && (object)value1 != null))
            {
                while (length >= 8)
                {
                    length -= 8;

                    lookUp = Unsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found7;
                    lookUp = Unsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found6;
                    lookUp = Unsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found5;
                    lookUp = Unsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found4;
                    lookUp = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }

                if (length >= 4)
                {
                    length -= 4;

                    lookUp = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }

                while (length > 0)
                {
                    length--;

                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp))
                        goto Found;
                }
            }
            else
            {
                for (length--; length >= 0; length--)
                {
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if ((object?)lookUp is null)
                    {
                        if ((object?)value0 is null || (object?)value1 is null)
                        {
                            goto Found;
                        }
                    }
                    else if (lookUp.Equals(value0!) || lookUp.Equals(value1!))
                    {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        //public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        //{
        //    while (length >= 8)
        //    {
        //        length -= 8;
        //        T other1 = Unsafe.Add<T>(ref searchSpace, length + 7);
        //        if (value0.Equals(other1) || value1.Equals(other1))
        //            return length + 7;
        //        T other2 = Unsafe.Add<T>(ref searchSpace, length + 6);
        //        if (value0.Equals(other2) || value1.Equals(other2))
        //            return length + 6;
        //        T other3 = Unsafe.Add<T>(ref searchSpace, length + 5);
        //        if (value0.Equals(other3) || value1.Equals(other3))
        //            return length + 5;
        //        T other4 = Unsafe.Add<T>(ref searchSpace, length + 4);
        //        if (value0.Equals(other4) || value1.Equals(other4))
        //            return length + 4;
        //        T other5 = Unsafe.Add<T>(ref searchSpace, length + 3);
        //        if (!value0.Equals(other5) && !value1.Equals(other5))
        //        {
        //            T other6 = Unsafe.Add<T>(ref searchSpace, length + 2);
        //            if (!value0.Equals(other6) && !value1.Equals(other6))
        //            {
        //                T other7 = Unsafe.Add<T>(ref searchSpace, length + 1);
        //                if (!value0.Equals(other7) && !value1.Equals(other7))
        //                {
        //                    T other8 = Unsafe.Add<T>(ref searchSpace, length);
        //                    if (value0.Equals(other8) || value1.Equals(other8))
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
        //    if (length >= 4)
        //    {
        //        length -= 4;
        //        T other1 = Unsafe.Add<T>(ref searchSpace, length + 3);
        //        if (!value0.Equals(other1) && !value1.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, length + 2);
        //            if (!value0.Equals(other2) && !value1.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, length + 1);
        //                if (!value0.Equals(other3) && !value1.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, length);
        //                    if (value0.Equals(other4) || value1.Equals(other4))
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
        //    while (length > 0)
        //    {
        //        --length;
        //        T other = Unsafe.Add<T>(ref searchSpace, length);
        //        if (value0.Equals(other) || value1.Equals(other))
        //            goto label_18;
        //    }
        //    return -1;
        //label_18:
        //    return length;
        //label_19:
        //    return length + 1;
        //label_20:
        //    return length + 2;
        //label_21:
        //    return length + 3;
        //}


        public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            T lookUp;
            if (default(T) != null || ((object)value0 != null && (object)value1 != null))
            {
                while (length >= 8)
                {
                    length -= 8;

                    lookUp = Unsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found7;
                    lookUp = Unsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found6;
                    lookUp = Unsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found5;
                    lookUp = Unsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found4;
                    lookUp = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }

                if (length >= 4)
                {
                    length -= 4;

                    lookUp = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found3;
                    lookUp = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found2;
                    lookUp = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found1;
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }

                while (length > 0)
                {
                    length--;

                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if (value0.Equals(lookUp) || value1.Equals(lookUp) || value2.Equals(lookUp))
                        goto Found;
                }
            }
            else
            {
                for (length--; length >= 0; length--)
                {
                    lookUp = Unsafe.Add(ref searchSpace, length);
                    if ((object?)lookUp is null)
                    {
                        if ((object?)value0 is null || (object?)value1 is null || (object?)value2 is null)
                        {
                            goto Found;
                        }
                    }
                    else if (lookUp.Equals(value0!) || lookUp.Equals(value1!) || lookUp.Equals(value2))
                    {
                        goto Found;
                    }
                }
            }

            return -1;

        Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return length;
        Found1:
            return length + 1;
        Found2:
            return length + 2;
        Found3:
            return length + 3;
        Found4:
            return length + 4;
        Found5:
            return length + 5;
        Found6:
            return length + 6;
        Found7:
            return length + 7;
        }

        //public static int LastIndexOfAny<T>(
        //  ref T searchSpace,
        //  T value0,
        //  T value1,
        //  T value2,
        //  int length)
        //  where T : IEquatable<T>
        //{
        //    while (length >= 8)
        //    {
        //        length -= 8;
        //        T other1 = Unsafe.Add<T>(ref searchSpace, length + 7);
        //        if (value0.Equals(other1) || value1.Equals(other1) || value2.Equals(other1))
        //            return length + 7;
        //        T other2 = Unsafe.Add<T>(ref searchSpace, length + 6);
        //        if (value0.Equals(other2) || value1.Equals(other2) || value2.Equals(other2))
        //            return length + 6;
        //        T other3 = Unsafe.Add<T>(ref searchSpace, length + 5);
        //        if (value0.Equals(other3) || value1.Equals(other3) || value2.Equals(other3))
        //            return length + 5;
        //        T other4 = Unsafe.Add<T>(ref searchSpace, length + 4);
        //        if (value0.Equals(other4) || value1.Equals(other4) || value2.Equals(other4))
        //            return length + 4;
        //        T other5 = Unsafe.Add<T>(ref searchSpace, length + 3);
        //        if (!value0.Equals(other5) && !value1.Equals(other5) && !value2.Equals(other5))
        //        {
        //            T other6 = Unsafe.Add<T>(ref searchSpace, length + 2);
        //            if (!value0.Equals(other6) && !value1.Equals(other6) && !value2.Equals(other6))
        //            {
        //                T other7 = Unsafe.Add<T>(ref searchSpace, length + 1);
        //                if (!value0.Equals(other7) && !value1.Equals(other7) && !value2.Equals(other7))
        //                {
        //                    T other8 = Unsafe.Add<T>(ref searchSpace, length);
        //                    if (value0.Equals(other8) || value1.Equals(other8) || value2.Equals(other8))
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
        //    if (length >= 4)
        //    {
        //        length -= 4;
        //        T other1 = Unsafe.Add<T>(ref searchSpace, length + 3);
        //        if (!value0.Equals(other1) && !value1.Equals(other1) && !value2.Equals(other1))
        //        {
        //            T other2 = Unsafe.Add<T>(ref searchSpace, length + 2);
        //            if (!value0.Equals(other2) && !value1.Equals(other2) && !value2.Equals(other2))
        //            {
        //                T other3 = Unsafe.Add<T>(ref searchSpace, length + 1);
        //                if (!value0.Equals(other3) && !value1.Equals(other3) && !value2.Equals(other3))
        //                {
        //                    T other4 = Unsafe.Add<T>(ref searchSpace, length);
        //                    if (value0.Equals(other4) || value1.Equals(other4) || value2.Equals(other4))
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
        //    while (length > 0)
        //    {
        //        --length;
        //        T other = Unsafe.Add<T>(ref searchSpace, length);
        //        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
        //            goto label_18;
        //    }
        //    return -1;
        //label_18:
        //    return length;
        //label_19:
        //    return length + 1;
        //label_20:
        //    return length + 2;
        //label_21:
        //    return length + 3;
        //}

        public static int LastIndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return -1;  // A zero-length set of values is always treated as "not found".

            // See comments in IndexOfAny(ref T, int, ref T, int) above regarding algorithmic complexity concerns.
            // This logic is similar, but it runs backward.

            if (typeof(T).IsValueType)
            {
                for (int i = searchSpaceLength - 1; i >= 0; i--)
                {
                    T candidate = Unsafe.Add(ref searchSpace, i);
                    for (int j = 0; j < valueLength; j++)
                    {
                        if (Unsafe.Add(ref value, j).Equals(candidate))
                        {
                            return i;
                        }
                    }
                }
            }
            else
            {
                for (int i = searchSpaceLength - 1; i >= 0; i--)
                {
                    T candidate = Unsafe.Add(ref searchSpace, i);
                    if (candidate is not null)
                    {
                        for (int j = 0; j < valueLength; j++)
                        {
                            if (candidate.Equals(Unsafe.Add(ref value, j)))
                            {
                                return i;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < valueLength; j++)
                        {
                            if (Unsafe.Add(ref value, j) is null)
                            {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1; // not found
        }


        //public static int LastIndexOfAny<T>(
        //  ref T searchSpace,
        //  int searchSpaceLength,
        //  ref T value,
        //  int valueLength)
        //  where T : IEquatable<T>
        //{
        //    if (valueLength == 0)
        //        return 0;
        //    int num1 = -1;
        //    for (int elementOffset = 0; elementOffset < valueLength; ++elementOffset)
        //    {
        //        int num2 = SpanHelpers.LastIndexOf<T>(ref searchSpace, Unsafe.Add<T>(ref value, elementOffset), searchSpaceLength);
        //        if (num2 > num1)
        //            num1 = num2;
        //    }
        //    return num1;
        //}



        public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>
        {
            Debug.Assert(length >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
            T lookUp0;
            T lookUp1;
            while (length >= 8)
            {
                length -= 8;

                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 1);
                lookUp1 = Unsafe.Add(ref second, index + 1);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 2);
                lookUp1 = Unsafe.Add(ref second, index + 2);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 3);
                lookUp1 = Unsafe.Add(ref second, index + 3);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 4);
                lookUp1 = Unsafe.Add(ref second, index + 4);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 5);
                lookUp1 = Unsafe.Add(ref second, index + 5);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 6);
                lookUp1 = Unsafe.Add(ref second, index + 6);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 7);
                lookUp1 = Unsafe.Add(ref second, index + 7);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;

                index += 8;
            }

            if (length >= 4)
            {
                length -= 4;

                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 1);
                lookUp1 = Unsafe.Add(ref second, index + 1);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 2);
                lookUp1 = Unsafe.Add(ref second, index + 2);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                lookUp0 = Unsafe.Add(ref first, index + 3);
                lookUp1 = Unsafe.Add(ref second, index + 3);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;

                index += 4;
            }

            while (length > 0)
            {
                lookUp0 = Unsafe.Add(ref first, index);
                lookUp1 = Unsafe.Add(ref second, index);
                if (!(lookUp0?.Equals(lookUp1) ?? (object?)lookUp1 is null))
                    goto NotEqual;
                index += 1;
                length--;
            }

        Equal:
            return true;

        NotEqual: // Workaround for https://github.com/dotnet/runtime/issues/8795
            return false;
        }



        //public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>
        //{
        //    if (!Unsafe.AreSame<T>(ref first, ref second))
        //    {
        //        IntPtr elementOffset = (IntPtr)0;
        //        while (length >= 8)
        //        {
        //            length -= 8;
        //            if (Unsafe.Add<T>(ref first, elementOffset).Equals(Unsafe.Add<T>(ref second, elementOffset)) && Unsafe.Add<T>(ref first, elementOffset + 1).Equals(Unsafe.Add<T>(ref second, elementOffset + 1)) && (Unsafe.Add<T>(ref first, elementOffset + 2).Equals(Unsafe.Add<T>(ref second, elementOffset + 2)) && Unsafe.Add<T>(ref first, elementOffset + 3).Equals(Unsafe.Add<T>(ref second, elementOffset + 3))) && (Unsafe.Add<T>(ref first, elementOffset + 4).Equals(Unsafe.Add<T>(ref second, elementOffset + 4)) && Unsafe.Add<T>(ref first, elementOffset + 5).Equals(Unsafe.Add<T>(ref second, elementOffset + 5)) && (Unsafe.Add<T>(ref first, elementOffset + 6).Equals(Unsafe.Add<T>(ref second, elementOffset + 6)) && Unsafe.Add<T>(ref first, elementOffset + 7).Equals(Unsafe.Add<T>(ref second, elementOffset + 7)))))
        //                elementOffset += 8;
        //            else
        //                goto label_12;
        //        }
        //        if (length >= 4)
        //        {
        //            length -= 4;
        //            if (Unsafe.Add<T>(ref first, elementOffset).Equals(Unsafe.Add<T>(ref second, elementOffset)) && Unsafe.Add<T>(ref first, elementOffset + 1).Equals(Unsafe.Add<T>(ref second, elementOffset + 1)) && (Unsafe.Add<T>(ref first, elementOffset + 2).Equals(Unsafe.Add<T>(ref second, elementOffset + 2)) && Unsafe.Add<T>(ref first, elementOffset + 3).Equals(Unsafe.Add<T>(ref second, elementOffset + 3))))
        //                elementOffset += 4;
        //            else
        //                goto label_12;
        //        }
        //        for (; length > 0; --length)
        //        {
        //            if (Unsafe.Add<T>(ref first, elementOffset).Equals(Unsafe.Add<T>(ref second, elementOffset)))
        //                elementOffset += 1;
        //            else
        //                goto label_12;
        //        }
        //        goto label_11;
        //    label_12:
        //        return false;
        //    }
        //label_11:
        //    return true;
        //}


        public static int SequenceCompareTo<T>(ref T first, int firstLength, ref T second, int secondLength)
            where T : IComparable<T>
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            int minLength = firstLength;
            if (minLength > secondLength)
                minLength = secondLength;
            for (int i = 0; i < minLength; i++)
            {
                T lookUp = Unsafe.Add(ref second, i);
                int result = (Unsafe.Add(ref first, i)?.CompareTo(lookUp) ?? (((object?)lookUp is null) ? 0 : -1));
                if (result != 0)
                    return result;
            }
            return firstLength.CompareTo(secondLength);
        }

        //public static int SequenceCompareTo<T>(
        //  ref T first,
        //  int firstLength,
        //  ref T second,
        //  int secondLength)
        //  where T : IComparable<T>
        //{
        //    int num1 = firstLength;
        //    if (num1 > secondLength)
        //        num1 = secondLength;
        //    for (int elementOffset = 0; elementOffset < num1; ++elementOffset)
        //    {
        //        int num2 = Unsafe.Add<T>(ref first, elementOffset).CompareTo(Unsafe.Add<T>(ref second, elementOffset));
        //        if (num2 != 0)
        //            return num2;
        //    }
        //    return firstLength.CompareTo(secondLength);
        //}



    }
}
