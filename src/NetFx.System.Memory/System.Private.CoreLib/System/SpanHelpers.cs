// Decompiled with JetBrains decompiler
// Type: System.SpanHelpers
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    internal static partial class SpanHelpers
    {
        public static unsafe void CopyTo<T>(ref T dst, int dstLength, ref T src, int srcLength)
        {
            IntPtr num1 = Unsafe.ByteOffset<T>(ref src, ref Unsafe.Add<T>(ref src, srcLength));
            IntPtr num2 = Unsafe.ByteOffset<T>(ref dst, ref Unsafe.Add<T>(ref dst, dstLength));
            IntPtr num3 = Unsafe.ByteOffset<T>(ref src, ref dst);
            if (!(sizeof(IntPtr) == 4 ? (uint)(int)num3 < (uint)(int)num1 || (uint)(int)num3 > (uint)-(int)num2 : (ulong)(long)num3 < (ulong)(long)num1 || (ulong)(long)num3 > (ulong)-(long)num2) && !SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                ref byte local1 = ref Unsafe.As<T, byte>(ref dst);
                ref byte local2 = ref Unsafe.As<T, byte>(ref src);
                ulong num4 = (ulong)(long)num1;
                uint byteCount;
                for (ulong index = 0; index < num4; index += (ulong)byteCount)
                {
                    byteCount = num4 - index > (ulong)uint.MaxValue ? uint.MaxValue : (uint)(num4 - index);
                    Unsafe.CopyBlock(ref Unsafe.Add<byte>(ref local1, (IntPtr)(long)index), ref Unsafe.Add<byte>(ref local2, (IntPtr)(long)index), byteCount);
                }
            }
            else
            {
                bool flag = sizeof(IntPtr) == 4 ? (uint)(int)num3 > (uint)-(int)num2 : (ulong)(long)num3 > (ulong)-(long)num2;
                int num4 = flag ? 1 : -1;
                int elementOffset = flag ? 0 : srcLength - 1;
                int num5;
                for (num5 = 0; num5 < (srcLength & -8); num5 += 8)
                {
                    Unsafe.Add<T>(ref dst, elementOffset) = Unsafe.Add<T>(ref src, elementOffset);
                    Unsafe.Add<T>(ref dst, elementOffset + num4) = Unsafe.Add<T>(ref src, elementOffset + num4);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 2) = Unsafe.Add<T>(ref src, elementOffset + num4 * 2);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 3) = Unsafe.Add<T>(ref src, elementOffset + num4 * 3);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 4) = Unsafe.Add<T>(ref src, elementOffset + num4 * 4);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 5) = Unsafe.Add<T>(ref src, elementOffset + num4 * 5);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 6) = Unsafe.Add<T>(ref src, elementOffset + num4 * 6);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 7) = Unsafe.Add<T>(ref src, elementOffset + num4 * 7);
                    elementOffset += num4 * 8;
                }
                if (num5 < (srcLength & -4))
                {
                    Unsafe.Add<T>(ref dst, elementOffset) = Unsafe.Add<T>(ref src, elementOffset);
                    Unsafe.Add<T>(ref dst, elementOffset + num4) = Unsafe.Add<T>(ref src, elementOffset + num4);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 2) = Unsafe.Add<T>(ref src, elementOffset + num4 * 2);
                    Unsafe.Add<T>(ref dst, elementOffset + num4 * 3) = Unsafe.Add<T>(ref src, elementOffset + num4 * 3);
                    elementOffset += num4 * 4;
                    num5 += 4;
                }
                for (; num5 < srcLength; ++num5)
                {
                    Unsafe.Add<T>(ref dst, elementOffset) = Unsafe.Add<T>(ref src, elementOffset);
                    elementOffset += num4;
                }
            }
        }

        public unsafe static IntPtr Add<T>(this IntPtr start, int index)
        {
            if (sizeof(IntPtr) == 4)
            {
                uint num = (uint)(index * Unsafe.SizeOf<T>());
                return (IntPtr)((byte*)(void*)start + num);
            }

            ulong num2 = (ulong)index * (ulong)Unsafe.SizeOf<T>();
            return (IntPtr)((byte*)(void*)start + num2);
        }

        public static bool IsReferenceOrContainsReferences<T>()
        {
            return PerTypeValues<T>.IsReferenceOrContainsReferences;
        }

        private static bool IsReferenceOrContainsReferencesCore(Type type)
        {
            if (type.IsPrimitive)
            {
                return false;
            }

            if (!type.IsValueType)
            {
                return true;
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if ((object)underlyingType != null)
            {
                type = underlyingType;
            }

            if (type.IsEnum)
            {
                return false;
            }

            FieldInfo[] declaredFields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (FieldInfo declaredField in declaredFields)
            {
                if (!declaredField.IsStatic && IsReferenceOrContainsReferencesCore(declaredField.FieldType))
                {
                    return true;
                }
            }

            return false;
        }

        public static unsafe void ClearLessThanPointerSized(byte* ptr, UIntPtr byteLength)
        {
            if (sizeof(UIntPtr) == 4)
            {
                Unsafe.InitBlockUnaligned((void*)ptr, (byte)0, (uint)byteLength);
            }
            else
            {
                ulong num1 = (ulong)byteLength;
                uint byteCount1 = (uint)(num1 & (ulong)uint.MaxValue);
                Unsafe.InitBlockUnaligned((void*)ptr, (byte)0, byteCount1);
                ulong num2 = num1 - (ulong)byteCount1;
                ptr += byteCount1;
                uint byteCount2;
                for (; num2 > 0UL; num2 -= (ulong)byteCount2)
                {
                    byteCount2 = num2 >= (ulong)uint.MaxValue ? uint.MaxValue : (uint)num2;
                    Unsafe.InitBlockUnaligned((void*)ptr, (byte)0, byteCount2);
                    ptr += byteCount2;
                }
            }
        }

        public static unsafe void ClearLessThanPointerSized(ref byte b, UIntPtr byteLength)
        {
            if (sizeof(UIntPtr) == 4)
            {
                Unsafe.InitBlockUnaligned(ref b, (byte)0, (uint)byteLength);
            }
            else
            {
                ulong num1 = (ulong)byteLength;
                uint byteCount1 = (uint)(num1 & (ulong)uint.MaxValue);
                Unsafe.InitBlockUnaligned(ref b, (byte)0, byteCount1);
                ulong num2 = num1 - (ulong)byteCount1;
                long num3 = (long)byteCount1;
                uint byteCount2;
                for (; num2 > 0UL; num2 -= (ulong)byteCount2)
                {
                    byteCount2 = num2 >= (ulong)uint.MaxValue ? uint.MaxValue : (uint)num2;
                    Unsafe.InitBlockUnaligned(ref Unsafe.Add<byte>(ref b, (IntPtr)num3), (byte)0, byteCount2);
                    num3 += (long)byteCount2;
                }
            }
        }

        public static unsafe void ClearPointerSizedWithoutReferences(ref byte b, UIntPtr byteLength)
        {
            IntPtr i;
            for (i = IntPtr.Zero; i.LessThanEqual(byteLength - sizeof(Reg64)); i += sizeof(Reg64))
            {
                Unsafe.As<byte, Reg64>(ref Unsafe.Add(ref b, i)) = new Reg64();
            }
            if (i.LessThanEqual(byteLength - sizeof(Reg32)))
            {
                Unsafe.As<byte, Reg32>(ref Unsafe.Add(ref b, i)) = new Reg32();
                i += sizeof(Reg32);
            }
            if (i.LessThanEqual(byteLength - sizeof(Reg16)))
            {
                Unsafe.As<byte, Reg16>(ref Unsafe.Add(ref b, i)) = new Reg16();
                i += sizeof(Reg16);
            }
            if (i.LessThanEqual(byteLength - 8))
            {
                Unsafe.As<byte, long>(ref Unsafe.Add(ref b, i)) = (long)0;
                i += 8;
            }
            if (sizeof(IntPtr) == 4 && i.LessThanEqual(byteLength - 4))
            {
                Unsafe.As<byte, int>(ref Unsafe.Add(ref b, i)) = 0;
                i += 4;
            }
        }

        public static void ClearPointerSizedWithReferences(ref IntPtr ip, UIntPtr pointerSizeLength)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr intPtr = IntPtr.Zero;
            while (true)
            {
                IntPtr intPtr1 = zero + 8;
                intPtr = intPtr1;
                if (!intPtr1.LessThanEqual(pointerSizeLength))
                {
                    break;
                }
                Unsafe.Add<IntPtr>(ref ip, zero + 0) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 1) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 2) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 3) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 4) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 5) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 6) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 7) = new IntPtr();
                zero = intPtr;
            }
            IntPtr intPtr2 = zero + 4;
            intPtr = intPtr2;
            if (intPtr2.LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add<IntPtr>(ref ip, zero + 0) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 1) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 2) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 3) = new IntPtr();
                zero = intPtr;
            }
            IntPtr intPtr3 = zero + 2;
            intPtr = intPtr3;
            if (intPtr3.LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add<IntPtr>(ref ip, zero + 0) = new IntPtr();
                Unsafe.Add<IntPtr>(ref ip, zero + 1) = new IntPtr();
                zero = intPtr;
            }
            if ((zero + 1).LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add<IntPtr>(ref ip, zero) = new IntPtr();
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool LessThanEqual(this IntPtr index, UIntPtr length)
        {
            return sizeof(UIntPtr) != 4 ? (long)index <= (long)(ulong)length : (int)index <= (int)(uint)length;
        }

        internal struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
        {
            private readonly T _value;
            private readonly TComparer _comparer;

            public ComparerComparable(T value, TComparer comparer)
            {
                _value = value;
                _comparer = comparer;
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(T other)
            {
                return _comparer.Compare(_value, other);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Reg64
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Reg32
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Reg16
        {
        }

        public static class PerTypeValues<T>
        {
            public static readonly bool IsReferenceOrContainsReferences = IsReferenceOrContainsReferencesCore(typeof(T));
            public static readonly T[] EmptyArray = new T[0];
            public static readonly IntPtr ArrayAdjustment = PerTypeValues<T>.MeasureArrayAdjustment();

            private static IntPtr MeasureArrayAdjustment()
            {
                T[] objArray = new T[1];
                return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<T>>(objArray).Data!, ref objArray[0]);
            }
        }
    }
}
