using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonLocalization(options => options.ResourcesPath = ["Resources"]);

var app = builder.Build();

app.UseRequestLocalization(options
    => options.AddSupportedCultures("en-US", "fr-FR")
    .AddSupportedUICultures("en-US", "fr-FR")
    .SetDefaultCulture("en-US")
);

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
