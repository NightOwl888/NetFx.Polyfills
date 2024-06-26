﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace System.SpanTests
{
    public static partial class MemoryMarshalTests
    {
        [Fact]
        public static void Span_AsBytesUIntToByte()
        {
            uint[] a = { 0x44332211, 0x88776655 };
            Span<uint> span = new Span<uint>(a);
            Span<byte> asBytes = MemoryMarshal.AsBytes<uint>(span);

            Assert.True(Unsafe.AreSame<byte>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(span)), ref MemoryMarshal.GetReference(asBytes)));
            asBytes.Validate<byte>(0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88);
        }

        [Fact]
        public static void Span_AsBytesContainsReferences()
        {
            Span<TestHelpers.StructWithReferences> span = new Span<TestHelpers.StructWithReferences>(Array.Empty<TestHelpers.StructWithReferences>());
            TestHelpers.AssertThrows<ArgumentException, TestHelpers.StructWithReferences>(span, (_span) => MemoryMarshal.AsBytes(_span).DontBox());
        }
    }
}