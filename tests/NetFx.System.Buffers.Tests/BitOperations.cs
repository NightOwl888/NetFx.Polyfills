using System;

namespace System.Numerics
{
    public class BitOperations
    {
        /// <summary>Round the given integral value up to a power of 2.</summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
        /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
        /// </returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[CLSCompliant(false)]
        public static uint RoundUpToPowerOf2(uint value)
        {
//            if (Lzcnt.IsSupported || ArmBase.IsSupported || X86Base.IsSupported)
//            {
//#if TARGET_64BIT
//                return (uint)(0x1_0000_0000ul >> LeadingZeroCount(value - 1));
//#else
//                int shift = 32 - LeadingZeroCount(value - 1);
//                return (1u ^ (uint)(shift >> 5)) << shift;
//#endif
//            }

            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }
    }
}
