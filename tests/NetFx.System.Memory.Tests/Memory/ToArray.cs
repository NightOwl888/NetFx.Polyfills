﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.MemoryTests
{
    public static partial class MemoryTests
    {
        [Fact]
        public static void ToArray1()
        {
            int[] a = { 91, 92, 93 };
            var memory = new Memory<int>(a);
            int[] copy = memory.ToArray();
            Assert.Equal<int>(a, copy);
            Assert.NotSame(a, copy);
        }

        [Fact]
        public static void ToArrayWithIndex()
        {
            int[] a = { 91, 92, 93, 94, 95 };
            var memory = new Memory<int>(a);
            int[] copy = memory.Slice(2).ToArray();

            Assert.Equal<int>(new int[] { 93, 94, 95 }, copy);
        }

        [Fact]
        public static void ToArrayWithIndexAndLength()
        {
            int[] a = { 91, 92, 93 };
            var memory = new Memory<int>(a, 1, 1);
            int[] copy = memory.ToArray();
            Assert.Equal<int>(new int[] { 92 }, copy);
        }

        [Fact]
        public static void ToArrayEmpty()
        {
            Memory<int> memory = Memory<int>.Empty;
            int[] copy = memory.ToArray();
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, copy.Length);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }

        [Fact]
        public static void ToArrayDefault()
        {
            Memory<int> memory = default;
            int[] copy = memory.ToArray();
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
            Assert.Equal(0, copy.Length);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        }
    }
}