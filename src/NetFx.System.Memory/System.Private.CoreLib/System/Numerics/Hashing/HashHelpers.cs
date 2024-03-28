// Decompiled with JetBrains decompiler
// Type: System.Numerics.Hashing.HashHelpers
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168

namespace System.Numerics.Hashing
{
    internal static class HashHelpers
    {
        public static readonly int RandomSeed = Guid.NewGuid().GetHashCode();

        public static int Combine(int h1, int h2)
        {
            return (int)((uint)(h1 << 5) | (uint)h1 >> 27) + h1 ^ h2;
        }
    }
}
