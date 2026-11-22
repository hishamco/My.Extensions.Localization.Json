using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonLocalization(options
    => options.ResourcesPath = new[] { "Resources" });

var app = builder.Build();

var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR")
};
var options = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(options);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", async (context) =>
{
    var localizer1 = context.RequestServices.GetRequiredService<IStringLocalizer>();
    var localizer2 = context.RequestServices.GetRequiredService<IStringLocalizer<Program>>();

    await context.Response.WriteAsync($"{localizer1["Hello"]}!!");
    await context.Response.WriteAsync($"{localizer2["Hello"]}!!");
});

await app.RunAsync();

partial class Program
{
    // Just for strongly typed resource
    public string Hello { get; set; }
}
