// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace System.SpanTests
{
    public static partial class ReadOnlySpanTests
    {
        [Fact]
        public static void SliceInt()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlySpan<int> span = new ReadOnlySpan<int>(a).Slice(6);
            Assert.Equal(4, span.Length);
            Assert.True(Unsafe.AreSame(ref a[6], ref Unsafe.AsRef(in MemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntPastEnd()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlySpan<int> span = new ReadOnlySpan<int>(a).Slice(a.Length);
            Assert.Equal(0, span.Length);
            Assert.True(Unsafe.AreSame(ref a[a.Length - 1], ref Unsafe.Subtract<int>(ref Unsafe.AsRef(in MemoryMarshal.GetReference(span)), 1)));
        }

        [Fact]
        public static void SliceIntInt()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlySpan<int> span = new ReadOnlySpan<int>(a).Slice(3, 5);
            Assert.Equal(5, span.Length);
            Assert.True(Unsafe.AreSame(ref a[3], ref Unsafe.AsRef(in MemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntIntUpToEnd()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlySpan<int> span = new ReadOnlySpan<int>(a).Slice(4, 6);
            Assert.Equal(6, span.Length);
            Assert.True(Unsafe.AreSame(ref a[4], ref Unsafe.AsRef(in MemoryMarshal.GetReference(span))));
        }

        [Fact]
        public static void SliceIntIntPastEnd()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            ReadOnlySpan<int> span = new ReadOnlySpan<int>(a).Slice(a.Length, 0);
            Assert.Equal(0, span.Length);
            Assert.True(Unsafe.AreSame(ref a[a.Length - 1], ref Unsafe.Subtract<int>(ref Unsafe.AsRef(in MemoryMarshal.GetReference(span)), 1)));
        }

        [Fact]
        public static void SliceIntRangeChecksd()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(-1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(a.Length + 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(-1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(0, a.Length + 1).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(2, a.Length + 1 - 2).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(a.Length + 1, 0).DontBox());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlySpan<int>(a).Slice(a.Length, 1).DontBox());
        }
    }
}
