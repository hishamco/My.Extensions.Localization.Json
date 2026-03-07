# My.Extensions.Localization.Json

JSON Localization Resources for ASP.NET Core.

NuGet Package: [![NuGet](https://img.shields.io/nuget/v/My.Extensions.Localization.Json.svg)](https://www.nuget.org/packages/My.Extensions.Localization.Json/4.0.0)

Build Status: [![Build status](https://github.com/hishamco/My.Extensions.Localization.Json/actions/workflows/build.yml/badge.svg)](https://github.com/hishamco/My.Extensions.Localization.Json/actions?query=workflow%3A%22My.Extensions.Localization.Json%22)

Donation: [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate?hosted_button_id=56FYKDA477LU6)

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

Your localization resource should placed based on `ResourcesPath` folder, similar to the default .resx-based localization, but using JSON files instead. The file naming convention depends on the `ResourcesType` configuration.

The resource file should be valid JSON objects with key-value pairs, each representing a localized string:

```json
{
  "Hello": "Bonjour",
  "WelcomeMessage": "Bienvenue sur notre application !"
}
```

#### 2.1 Type-Based

The resource files are named based on the type's that uses the `IStringLocalizer`. For more information please refer to the [Resource file naming](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/provide-resources?view=aspnetcore-10.0#resource-file-naming) in ASP.NET Core Globalization and localizations docs.

#### 2.2 Culture-Based

The resource files are named based on the supported cultures, for example `ar.json`.

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
