System.Buffers Polyfill for .NET Framework 4.0
========

[![Nuget](https://img.shields.io/nuget/dt/NetFx.System.Buffers)](https://www.nuget.org/packages/NetFx.System.Buffers)
[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/NightOwl888/NetFx.Polyfills/4/main)](https://dev.azure.com/NightOwl888/NetFx.Polyfills/_build?definitionId=4&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/NetFx.Polyfills)](https://github.com/NightOwl888/NetFx.Polyfills/blob/main/LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

Provides resource pooling of any type for performance-critical applications that allocate and deallocate objects frequently.

Commonly Used Types:
- System.Buffers.ArrayPool&lt;T&gt;

--------------

This package adds support for System.Buffers types on .NET Framework 4.0.

As the source code is not available for the System.Buffers package that works on .NET Framework 4.5, types in this package have either been decompiled from [System.Buffers 4.5.1](https://www.nuget.org/packages/System.Buffers/4.5.1) or have been sourced from newer versions of .NET. Most of the tests from the last release of .NET Core 3.1 (version 3.1.29) have been ported and are all passing.

This is not meant to be an upgrade to System.Buffers 4.5.1, it is simply to add support on the `net40` target for all of the existing APIs in System.Buffers 4.5.1. It is recommended to use the official release of System.Buffers on newer versions of .NET.

## Interop with System.Buffers on Targets > net40

Since the runtime for `net40` hasn't been supported for many years, most likely you will be using a newer runtime with this library. But you may be interoperating with other components that target System.Runtime.CompilerServices.Unsafe, which will cause type collisions by default.

In this case, it is recommended to remove System.Buffers from compilation and add NetFx.System.Runtime.CompilerServices.Unsafe in its place. Add the following to your `.csproj` or `.vbproj` file. This example is for using `net452`.

```xml
  <ItemGroup Condition=" '$(TargetFramework)' == 'net452' ">
    <!-- ExcludeAssets=compile removes the dependency from being referenced.
         ExcludeAssets=runtime removes the dependency from the build output. -->
    <PackageReference Include="System.Buffers"
                      Version="4.5.1"
                      ExcludeAssets="compile;runtime" />
    <PackageReference Include="NetFx.System.Buffers"
                      Version="4.0.0" />
  </ItemGroup>
```

> **NOTE:** Only SDK-style projects are supported using this method.

For transitive dependencies (that is, dependencies that are not directly referenced) that have a `net40` target, consider [forcing a specific target framework](https://duanenewman.net/blog/post/forcing-a-specific-target-platform-with-packagereference/).

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/NetFx.Polyfills) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)