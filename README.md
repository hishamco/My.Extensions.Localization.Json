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
Resources are matched based on the class's full name (excluding the root namespace) or relative to the `ResourcesPath`.

If your class is `MyApp.Controllers.HomeController` and the root namespace is `MyApp`:

- **Option 1: Dot-separated in ResourcesPath**
  - `Resources/Controllers.HomeController.fr-FR.json`
  - `Resources/Controllers.HomeController.fr.json`

- **Option 2: Folder-based structure**
  - `Resources/Controllers/HomeController.fr-FR.json`
  - `Resources/Controllers/HomeController.fr.json`

The localizer will first look for the dot-separated file in the root `ResourcesPath`, and if not found, it will look in the folder-based structure. It also supports culture fallback (e.g., if `fr-FR` is requested but not found, it can use `fr`).

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

You can use the IStringLocalizer normally in you application.

## Configuration Options

- `ResourcesPath`: An array of paths where the localizer will look for JSON files.
- `ResourcesType`: 
    - `TypeBased`: (Default) Look for files based on the type's name relative to the assembly's root namespace (e.g., `Controllers.HomeController.fr.json` or `Controllers/HomeController.fr.json`).
    - `CultureBased`: Look for files named after the culture (e.g., `en-US.json`).
