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

### 1. Register Services

In `Program.cs` (or `Startup.cs` for older versions), add the JSON localization services:

```csharp
builder.Services.AddJsonLocalization(options => 
{
    options.ResourcesPath = new[] { "Resources" };
    options.ResourcesType = ResourcesType.TypeBased;
});
```

### 2. Create Resource Files

Depending on the `ResourcesType` selected, your file structure will differ:

#### Type-Based (Default)
Resources are matched based on the class's full name or relative to the `ResourcesPath`.

- **Resource Path**: `Resources/Controllers.HomeController.fr-FR.json`
- **Or**: `Resources/Controllers/HomeController.fr-FR.json`

#### Culture-Based
Resources are matched based on the culture name only.

- **Resource Path**: `Resources/fr-FR.json`

### 3. Resource File Content

The resource files should be valid JSON objects with key-value pairs:

```json
{
  "Hello": "Bonjour",
  "WelcomeMessage": "Bienvenue sur notre application !"
}
```

### 4. Use in Your Application

#### In Controllers

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
        ViewData["Title"] = _localizer["Hello"];
        return View();
    }
}
```

#### In Razor Views

```razor
@using Microsoft.AspNetCore.Mvc.Localization
@inject IViewLocalizer Localizer

<h1>@Localizer["Hello"]</h1>
```

#### In Data Annotations

```csharp
public class RegisterViewModel
{
    [Required(ErrorMessage = "The Email field is required.")]
    [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; }
}
```

Register DataAnnotations localization in `Program.cs`:

```csharp
builder.Services.AddMvc()
    .AddDataAnnotationsLocalization();
```

## Configuration Options

- `ResourcesPath`: An array of paths where the localizer will look for JSON files.
- `ResourcesType`: 
    - `TypeBased`: (Default) Look for files based on the type's full name or dot-separated path.
    - `CultureBased`: Look for files named after the culture (e.g., `en-US.json`).
