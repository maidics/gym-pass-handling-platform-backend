using System.Text.Json.Serialization;
using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Data;
using FitPass.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace FitPass.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
        {
            builder.Configuration.AddJsonFile(
                "secrets.local.json",
                optional: false,
                reloadOnChange: true
            );
        }

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true
        );

        builder.Services.AddEndpointsApiExplorer();

        //Json - Enum converter: strings only
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)
            );
        });

        builder.Services.AddOpenApiDocument(
            (configure, sp) =>
            {
                configure.Title = "FitPass API";

                // Add JWT
                configure.AddSecurity(
                    "JWT",
                    Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}.",
                    }
                );

                configure.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT")
                );
            }
        );

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                corsBuilder =>
                {
                    corsBuilder
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });

        builder.Services.AddScoped<StripeWebHookSignatureFilter>();

        //Server Sent Event service:
        builder.Services.AddSingleton<ClientNotificationService>();

        builder.Services.AddSingleton<IClientNotificationSender>(provider =>
            provider.GetRequiredService<ClientNotificationService>()
        );

        builder.Services.AddSingleton<IClientNotificationStreamer>(provider =>
            provider.GetRequiredService<ClientNotificationService>()
        );
    }
}
