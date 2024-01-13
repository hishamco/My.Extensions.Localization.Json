using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddJsonLocalization(options => options.ResourcesPath = "Resources");

CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

await builder.Build().RunAsync();
