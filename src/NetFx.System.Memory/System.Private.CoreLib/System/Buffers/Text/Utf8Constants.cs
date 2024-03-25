// Decompiled with JetBrains decompiler
// Type: System.Buffers.Text.Utf8Constants
// Assembly: System.Memory, Version=4.0.1.2, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
// MVID: 866AE087-4753-44D8-B4C3-B8D9EAD86168
// Assembly location: F:\Users\shad\source\repos\CheckSystemMemoryDependencies\CheckSystemMemoryDependencies\bin\Debug\net45\System.Memory.dll

namespace System.Buffers.Text
{
    internal static class Utf8Constants
    {
        public static readonly TimeSpan s_nullUtcOffset = TimeSpan.MinValue;
        public const byte Colon = 58;
        public const byte Comma = 44;
        public const byte Minus = 45;
        public const byte Period = 46;
        public const byte Plus = 43;
        public const byte Slash = 47;
        public const byte Space = 32;
        public const byte Hyphen = 45;
        public const byte Separator = 44;
        public const int GroupSize = 3;
        public const int DateTimeMaxUtcOffsetHours = 14;
        public const int DateTimeNumFractionDigits = 7;
        public const int MaxDateTimeFraction = 9999999;
        public const ulong BillionMaxUIntValue = 4294967295000000000;
        public const uint Billion = 1000000000;
    }
}
