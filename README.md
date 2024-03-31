Polyfills for .NET Framework 4.0
========

[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/NetFx.Polyfills/44041e22-bd88-42a2-ad29-ee6859a5010e/1/main)](https://dev.azure.com/NightOwl888/NetFx.Polyfills/_build?definitionId=1&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/NetFx.Polyfills)](https://github.com/NightOwl888/NetFx.Polyfills/blob/main/LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

These are re-compilations of .NET sources to provide support on .NET Framework 4.0. They are compatible with the official versions although some of the sources may be newer than the official release and contain newer behaviors. The public API surface is the same as the original release.

Our goal is to provide support for newer APIs that Microsoft didn't make available on .NET Framework 4.0 so you can modernize your code even if you cannot yet upgrade. These libraries are also useful for class libraries that multi-target and would ordinarily be forced to add conditional compilation sections to their code.

------------------

## NuGet

| Package  | Link |
| ------------- | ------------- |
| **NetFx.System.Buffers**  | [![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Buffers)](https://www.nuget.org/packages/NetFx.System.Buffers) |
| **NetFx.System.Memory**  | [![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Memory)](https://www.nuget.org/packages/NetFx.System.Memory) |
| **NetFx.System.Runtime.CompilerServices.Unsafe** | [![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Runtime.CompilerServices.Unsafe)](https://www.nuget.org/packages/NetFx.System.Runtime.CompilerServices.Unsafe) |


## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/NetFx.Polyfills) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)