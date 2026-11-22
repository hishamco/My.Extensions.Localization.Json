using LocalizationSample.Mvc.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddJsonLocalization(options => options.ResourcesPath = new[] { "Resources" });
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options
        => options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(RegisterViewModel)));

var app = builder.Build();

app.UseRequestLocalization("en-US", "fr-FR");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

await app.RunAsync();
