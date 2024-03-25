// Decompiled with JetBrains decompiler
// Type: System.DecimalDecCalc
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

namespace System
{
    internal static class DecimalDecCalc
    {
        private static uint D32DivMod1E9(uint hi32, ref uint lo32)
        {
            ulong num = (ulong)hi32 << 32 | (ulong)lo32;
            lo32 = (uint)(num / 1000000000UL);
            return (uint)(num % 1000000000UL);
        }

        internal static uint DecDivMod1E9(ref MutableDecimal value)
        {
            return D32DivMod1E9(D32DivMod1E9(D32DivMod1E9(0U, ref value.High), ref value.Mid), ref value.Low);
        }

        internal static void DecAddInt32(ref MutableDecimal value, uint i)
        {
            if (!D32AddCarry(ref value.Low, i) || !D32AddCarry(ref value.Mid, 1U))
                return;
            D32AddCarry(ref value.High, 1U);
        }

        private static bool D32AddCarry(ref uint value, uint i)
        {
            uint num1 = value;
            uint num2 = num1 + i;
            value = num2;
            return num2 < num1 || num2 < i;
        }

        internal static void DecMul10(ref MutableDecimal value)
        {
            MutableDecimal d = value;
            DecShiftLeft(ref value);
            DecShiftLeft(ref value);
            DecAdd(ref value, d);
            DecShiftLeft(ref value);
        }

        private static void DecShiftLeft(ref MutableDecimal value)
        {
            uint num1 = ((int)value.Low & int.MinValue) != 0 ? 1U : 0U;
            uint num2 = ((int)value.Mid & int.MinValue) != 0 ? 1U : 0U;
            value.Low <<= 1;
            value.Mid = value.Mid << 1 | num1;
            value.High = value.High << 1 | num2;
        }

        private static void DecAdd(ref MutableDecimal value, MutableDecimal d)
        {
            if (D32AddCarry(ref value.Low, d.Low) && D32AddCarry(ref value.Mid, 1U))
                D32AddCarry(ref value.High, 1U);
            if (D32AddCarry(ref value.Mid, d.Mid))
                D32AddCarry(ref value.High, 1U);
            D32AddCarry(ref value.High, d.High);
        }
    }
}
