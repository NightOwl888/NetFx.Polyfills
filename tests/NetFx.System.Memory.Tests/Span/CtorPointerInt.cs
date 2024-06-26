// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace System.SpanTests
{
    public static partial class SpanTests
    {
        [Fact]
        public static void CtorPointerInt()
        {
            unsafe
            {
                int[] a = { 90, 91, 92 };
                fixed (int* pa = a)
                {
                    Span<int> span = new Span<int>(pa, 3);
                    span.Validate(90, 91, 92);
                    Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>(pa), ref MemoryMarshal.GetReference(span)));
                }
            }
        }

        [Fact]
        public static void CtorPointerNull()
        {
            unsafe
            {
                Span<int> span = new Span<int>((void*)null, 0);
                span.Validate();
                Assert.True(Unsafe.AreSame(ref Unsafe.AsRef<int>((void*)null), ref MemoryMarshal.GetReference(span)));
            }
        }

        [Fact]
        public static void CtorPointerRangeChecks()
        {
            unsafe
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    delegate ()
                    {
                        int i = 42;
                        Span<int> span = new Span<int>(&i, -1);
                    });
            }
        }

        [Fact]
        public static void CtorPointerNoContainsReferenceEnforcement()
        {
            unsafe
            {
                new Span<int>((void*)null, 0);
                new Span<int?>((void*)null, 0);
                AssertExtensions.Throws<ArgumentException>(null, () => new Span<object>((void*)null, 0).DontBox());
                AssertExtensions.Throws<ArgumentException>(null, () => new Span<TestHelpers.StructWithReferences>((void*)null, 0).DontBox());
            }
        }
    }
}
