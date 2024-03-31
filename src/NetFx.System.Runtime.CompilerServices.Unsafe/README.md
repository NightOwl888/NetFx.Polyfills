System.Runtime.CompilerServices.Unsafe Polyfill for .NET Framework 4.0
========

[![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Runtime.CompilerServices.Unsafe)](https://www.nuget.org/packages/NetFx.System.Runtime.CompilerServices.Unsafe)
[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/NightOwl888/NetFx.Polyfills/4/main)](https://dev.azure.com/NightOwl888/NetFx.Polyfills/_build?definitionId=4&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/NetFx.Polyfills)](https://github.com/NightOwl888/NetFx.Polyfills/blob/main/LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

Provides the System.Runtime.CompilerServices.Unsafe class, which provides generic, low-level functionality for manipulating pointers.

Commonly Used Types:
- System.Runtime.CompilerServices.Unsafe

------------

This package adds support for System.Runtime.CompilerServices.Unsafe on .NET Framework 4.0.

This is a compilation using the System.Runtime.CompilerServices.Unsafe source code from .NET Core 6.0.28. All tests are passing.

This is not meant to be an upgrade to System.Runtime.CompilerServices.Unsafe 6.0.0, it is simply to add support on the `net40` target for all of the existing APIs in System.Runtime.CompilerServices.Unsafe 6.0.0. It is recommended to use the official release of System.Runtime.CompilerServices.Unsafe on newer versions of .NET.

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/NetFx.Polyfills) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)