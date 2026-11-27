# My.Extensions.Localization.Json

JSON Localization Resources for ASP.NET Core.

NuGet Package: ![NuGet](https://img.shields.io/nuget/v/My.Extensions.Localization.Json) ![NuGet Downloads](https://img.shields.io/nuget/dt/My.Extensions.Localization.Json)

Build Status: [![Build status](https://github.com/hishamco/My.Extensions.Localization.Json/actions/workflows/build.yml/badge.svg)](https://github.com/hishamco/My.Extensions.Localization.Json/actions?query=workflow%3A%22My.Extensions.Localization.Json%22)

## Installation

```bash
dotnet add package My.Extensions.Localization.Json
```

## Usage

### 1. Register JSON Localization Services

Adds the JSON localization services into DI by adding `AddJsonLocalization` in `Program.cs` or `Startup.cs`, as follows:

```csharp
builder.Services.AddJsonLocalization(options => 
{
    options.ResourcesPath = new[] { "Resources" };
    options.ResourcesType = ResourcesType.TypeBased;
});
```

### 2. Create Resource Files

Your localization resource should be placed based on the `ResourcesPath` folder, similar to the default .resx-based localization, but using JSON files instead. The file naming convention depends on the `ResourcesType` configuration.

The resource file should be valid JSON objects with key-value pairs, each representing a localized string:

```json
{
  "Hello": "Bonjour",
  "WelcomeMessage": "Bienvenue sur notre application !"
}
```

#### 2.1 Type-Based

The resource files are named based on the types that use the `IStringLocalizer`. For more information, please refer to the [Resource file naming](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/provide-resources?view=aspnetcore-10.0#resource-file-naming) in the ASP.NET Core Globalization and localizations docs.

#### 2.2 Culture-Based

The resource files are named based on the supported cultures, for example, `ar.json`.

### 3. Use in Your Application

You can use the `IStringLocalizer` normally in your application.

```csharp
public class HomeController : Controller
{
    private readonly IStringLocalizer<HomeController> _localizer;

    public HomeController(IStringLocalizer<HomeController> localizer)
    {
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        ViewData["Message"] = _localizer["WelcomeMessage"];

        return View();
    }
}
```

## Benchmarks

Run the benchmarks in Release configuration with the .NET 10 SDK or later installed:

```powershell
dotnet run --configuration Release --project .\test\My.Extensions.Localization.Json.Benchmarks\My.Extensions.Localization.Json.Benchmarks.csproj
```

The benchmark host and its generated builds target .NET 10. The target override also scopes restore of the referenced library to .NET 10. BenchmarkDotNet requires optimized Release assemblies.

The resource manager comparison measures published versions 3.0.0 through 4.1.0. Each job selects its package using `BenchmarkLibraryVersion` instead of the local project reference. Only the cross-version resource manager benchmark is compiled in these historical builds, since the other benchmark classes use newer APIs. Generated restores explicitly use the repository's `NuGet.Config` to preserve package source mapping. Results are written to `BenchmarkDotNet.Artifacts\results` under the working directory.

### JsonResourceManagerBenchmarks

| Method                                       | Job     | Mean         | Error        | StdDev       | Allocated |
|--------------------------------------------- |-------- |-------------:|-------------:|-------------:|----------:|
| EvaluateGetResourceSetWithoutCultureFallback | v3.0.0  | 94,781.85 ns |   643.044 ns |   942.565 ns |   30921 B |
| EvaluateGetResourceSetWithoutCultureFallback | v3.1.0  | 34,407.88 ns |   276.451 ns |   396.478 ns |   12624 B |
| EvaluateGetResourceSetWithoutCultureFallback | v3.2.0  | 33,836.63 ns | 1,103.478 ns | 1,651.633 ns |   12696 B |
| EvaluateGetResourceSetWithoutCultureFallback | v3.3.0  |     41.26 ns |     1.907 ns |     2.854 ns |     136 B |
| EvaluateGetResourceSetWithoutCultureFallback | v3.4.0  |     55.98 ns |     1.806 ns |     2.702 ns |     192 B |
| EvaluateGetResourceSetWithoutCultureFallback | v4.0.0  |     71.39 ns |     2.623 ns |     3.926 ns |     192 B |
| EvaluateGetResourceSetWithoutCultureFallback | v4.1.0  |     69.30 ns |     2.385 ns |     3.570 ns |     192 B |
| EvaluateGetResourceSetWithoutCultureFallback | Current |     77.96 ns |     3.217 ns |     4.815 ns |     192 B |
|                                              |         |              |              |              |           |
| EvaluateGetResourceSetWithCultureFallback    | v3.0.0  | 92,820.71 ns |   909.002 ns | 1,332.403 ns |   32481 B |
| EvaluateGetResourceSetWithCultureFallback    | v3.1.0  | 34,940.69 ns |   324.278 ns |   465.070 ns |   14184 B |
| EvaluateGetResourceSetWithCultureFallback    | v3.2.0  | 35,468.15 ns | 1,231.626 ns | 1,843.439 ns |   14256 B |
| EvaluateGetResourceSetWithCultureFallback    | v3.3.0  |    429.57 ns |    19.026 ns |    28.478 ns |    1696 B |
| EvaluateGetResourceSetWithCultureFallback    | v3.4.0  |    737.90 ns |    29.427 ns |    44.045 ns |    1816 B |
| EvaluateGetResourceSetWithCultureFallback    | v4.0.0  |    494.27 ns |    20.261 ns |    30.326 ns |    1824 B |
| EvaluateGetResourceSetWithCultureFallback    | v4.1.0  |    493.11 ns |    22.508 ns |    33.689 ns |    1824 B |
| EvaluateGetResourceSetWithCultureFallback    | Current |    500.97 ns |    18.410 ns |    26.403 ns |    1824 B |

### JsonResourceManagerBenchmark

| Method                                       | Keys    | Mean     | Error    | StdDev   | Allocated |
|--------------------------------------------- |-------- |---------:|---------:|---------:|----------:|
| EvaluateGetResourceSetWithoutCultureFallback | 10      | 23.74 us | 0.470 us | 0.823 us |   1.87 KB |
| EvaluateGetResourceSetWithoutCultureFallback | 100     | 24.23 us | 0.478 us | 0.873 us |   1.88 KB |
| EvaluateGetResourceSetWithoutCultureFallback | 1000    | 23.68 us | 0.473 us | 0.828 us |   1.89 KB |
| EvaluateGetResourceSetWithoutCultureFallback | 10000   | 23.96 us | 0.479 us | 1.031 us |   1.91 KB |
| EvaluateGetResourceSetWithoutCultureFallback | 100000  | 24.17 us | 0.482 us | 0.881 us |   1.92 KB |
| EvaluateGetResourceSetWithoutCultureFallback | 1000000 | 25.05 us | 0.496 us | 1.068 us |   1.93 KB |
|                                              |         |          |          |          |           |
| EvaluateGetResourceSetWithCultureFallback    | 10      | 23.78 us | 0.473 us | 0.816 us |   1.87 KB |
| EvaluateGetResourceSetWithCultureFallback    | 100     | 23.94 us | 0.474 us | 0.947 us |   1.88 KB |
| EvaluateGetResourceSetWithCultureFallback    | 1000    | 24.10 us | 0.480 us | 0.841 us |   1.89 KB |
| EvaluateGetResourceSetWithCultureFallback    | 10000   | 23.98 us | 0.480 us | 0.865 us |   1.91 KB |
| EvaluateGetResourceSetWithCultureFallback    | 100000  | 25.10 us | 0.492 us | 0.766 us |   1.92 KB |
| EvaluateGetResourceSetWithCultureFallback    | 1000000 | 24.55 us | 0.489 us | 1.191 us |   1.93 KB |

### JsonResourceLoaderBenchmark

| Method                        | Mean        | Error     | StdDev    | Allocated  |
|------------------------------ |------------:|----------:|----------:|-----------:|
| EvaluateLoadingSmallResources |    32.58 us |  0.650 us |  1.156 us |   10.67 KB |
| EvaluateLoadingBigResources   | 1,430.02 us | 28.542 us | 75.688 us | 2412.91 KB |
