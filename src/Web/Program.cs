using FitPass.Application;
using FitPass.Application.Common.Settings;
using FitPass.Infrastructure;
using FitPass.Infrastructure.Common;
using FitPass.Infrastructure.Data;
using FitPass.Web;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

var cultureSettings = app.Configuration.GetSection(ConfigurationSections.Cultures).Get<CultureSettings>();
Guard.Against.Null(cultureSettings);
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(cultureSettings.DefaultCulture)
    .AddSupportedCultures(cultureSettings.SupportedCultures)
    .AddSupportedUICultures(cultureSettings.SupportedCultures);

localizationOptions.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];

app.UseRequestLocalization(localizationOptions);

app.Run();

public partial class Program { }
