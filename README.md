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
dotnet run --project .\test\My.Extensions.Localization.Json.Benchmarks\My.Extensions.Localization.Json.Benchmarks.csproj -c Release -f net10.0 -p:TargetFrameworks=net10.0 -- --filter "*JsonResourceManagerBenchmarks*"
```

The benchmark host and its generated builds target .NET 10. The target override also scopes restore of the referenced library to .NET 10. BenchmarkDotNet requires optimized Release assemblies.

The resource manager comparison measures published versions 3.0.0 through 4.1.0. Each job selects its package using `BenchmarkLibraryVersion` instead of the local project reference. Only the cross-version resource manager benchmark is compiled in these historical builds, since the other benchmark classes use newer APIs. Generated restores explicitly use the repository's `NuGet.Config` to preserve package source mapping. Results are written to `BenchmarkDotNet.Artifacts\results` under the working directory.

### JsonResourceManagerBenchmarks

| Method                                       | Job        | Mean         | Error        | StdDev       |
|--------------------------------------------- |----------- |-------------:|-------------:|-------------:|
| EvaluateGetResourceSetWithoutCultureFallback | v3.0.0     | 92,676.44 ns | 1,257.806 ns | 1,843.676 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v3.1.0     | 35,557.31 ns | 1,350.101 ns | 1,892.653 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v3.2.0     | 33,605.77 ns | 1,029.281 ns | 1,540.580 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v3.3.0     |     58.24 ns |     2.177 ns |     3.259 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v3.4.0     |     71.42 ns |     2.941 ns |     4.403 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v4.0.0     |    115.87 ns |     3.929 ns |     5.880 ns |
| EvaluateGetResourceSetWithoutCultureFallback | v4.1.0     |    118.46 ns |     4.490 ns |     6.721 ns |
| EvaluateGetResourceSetWithoutCultureFallback | Current    |    146.70 ns |     5.823 ns |     8.716 ns |
|                                              |            |              |              |              |
| EvaluateGetResourceSetWithCultureFallback    | v3.0.0     | 96,567.62 ns | 1,859.263 ns | 2,666.499 ns |
| EvaluateGetResourceSetWithCultureFallback    | v3.1.0     | 34,873.83 ns |   350.347 ns |   524.383 ns |
| EvaluateGetResourceSetWithCultureFallback    | v3.2.0     | 33,745.82 ns |   946.474 ns | 1,416.637 ns |
| EvaluateGetResourceSetWithCultureFallback    | v3.3.0     |    267.71 ns |     9.670 ns |    14.474 ns |
| EvaluateGetResourceSetWithCultureFallback    | v3.4.0     |    410.47 ns |    12.263 ns |    18.354 ns |
| EvaluateGetResourceSetWithCultureFallback    | v4.0.0     |    354.65 ns |    11.730 ns |    17.556 ns |
| EvaluateGetResourceSetWithCultureFallback    | v4.1.0     |    351.84 ns |    10.510 ns |    15.731 ns |
| EvaluateGetResourceSetWithCultureFallback    | Current    |    374.77 ns |    10.055 ns |    15.049 ns |

### JsonResourceManagerBenchmark

| Method                                       | Mean     | Error    | StdDev   | Gen0   | Allocated |
|----------------------------------------------|---------:|---------:|---------:|-------:|----------:|
| EvaluateGetResourceSetWithoutCultureFallback | 19.81 us | 0.221 us | 0.173 us | 0.0916 |   1.38 KB |
| EvaluateGetResourceSetWithCultureFallback    | 20.23 us | 0.384 us | 0.411 us | 0.0916 |   1.38 KB |

### JsonResourceLoaderBenchmark

| Method                        | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------------------------ |----------:|----------:|----------:|-------:|----------:|
| EvaluateLoadingSmallResources | 31.454 us | 0.6061 us | 0.8297 us | 0.7935 |   10688 B |
| EvaluateLoadingBigResources   |  3.444 us | 0.0670 us | 0.0717 us | 0.0267 |     360 B |

