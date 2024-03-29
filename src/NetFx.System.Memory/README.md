System.Memory Polyfill for .NET Framework 4.0
========

[![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Memory)](https://www.nuget.org/packages/NetFx.System.Memory)
[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/NetFx.Polyfills/44041e22-bd88-42a2-ad29-ee6859a5010e/1/main)](https://dev.azure.com/NightOwl888/NetFx.Polyfills/_build?definitionId=1&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/NetFx.Polyfills)](https://github.com/NightOwl888/NetFx.Polyfills/blob/main/LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

Provides types for efficient representation and pooling of managed, stack, and native memory segments and sequences of such segments, along with primitives to parse and format UTF-8 encoded text stored in those memory segments.

Commonly Used Types:
- System.Span
- System.ReadOnlySpan
- System.Memory
- System.ReadOnlyMemory
- System.Buffers.MemoryPool
- System.Buffers.ReadOnlySequence
- System.Buffers.Text.Utf8Parser
- System.Buffers.Text.Utf8Formatter
 
-------

This package adds support for System.Memory types on .NET Framework 4.0.

As the source code for the System.Memory package that works on .NET Framework 4.5 is not available, types in this package have either been decompiled from [System.Memory 4.5.5](https://www.nuget.org/packages/System.Buffers/4.5.1) or have been sourced from newer versions of .NET. Most of the tests from the last release of .NET Core 3.1 (version 3.1.29) have been ported and are all passing. There are 103 tests that pass on this library that don't pass on System.Memory, so it contains some behavior that is more in line with later versions of .NET than with System.Memory 4.5.5. But these are minor differences that most users won't notice.

This is not meant to be an upgrade to System.Memory 4.5.5, it is simply to add support on the `net40` target for all of the existing APIs in System.Memory 4.5.5. It is recommended to use the official release of System.Memory on newer versions of .NET.

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/NetFx.Polyfills) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)