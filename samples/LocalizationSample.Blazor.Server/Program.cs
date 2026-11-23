using LocalizationSample.Blazor.Server.Data;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

builder.Services.AddJsonLocalization(options => options.ResourcesPath = ["Resources"]);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization("en-US", "fr-FR");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
