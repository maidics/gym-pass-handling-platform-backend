using System.Net;
using System.Security.Claims;
using System.Text;
using Fitpass.Infrastructure.Data.Interceptors;
using Fitpass.Infrastructure.Services.Email;
using Fitpass.Infrastructure.Stripe.Services;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Data;
using FitPass.Infrastructure.Data.DbSeed;
using FitPass.Infrastructure.Data.Interceptors;
using FitPass.Infrastructure.Identity;
using FitPass.Infrastructure.Services;
using FitPass.Infrastructure.Services.Email;
using FitPass.Infrastructure.Services.Jwt;
using FitPass.Infrastructure.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Stripe;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("FitPassDb");
        Guard.Against.Null(connectionString, message: "Connection string 'FitPassDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<InterceptorStateService>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, FitPass.Infrastructure.Identity.IdentityService>();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
        builder.Services.AddTransient<ILocalDevEmailService, LocalDevEmailService>();

        builder.Services.AddTransient<IQrCodeService, QrCodeService>();

        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(StripeSettings.SectionName));

        builder.Services.AddScoped<CustomerService>();
        builder.Services.AddScoped<ProductService>();
        builder.Services.AddScoped<PriceService>();

        builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();
        builder.Services.AddScoped<IStripeProductService, StripeProductService>();
        builder.Services.AddScoped<IStripePriceService, StripePriceService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                string jwtSectionKey = "Jwt"; 
                var jwtSettings = builder.Configuration.GetSection(jwtSectionKey).Get<JwtSettings>();

                if (jwtSettings == null)
                {
                    throw new ArgumentException($"Jwt section not found with '{jwtSectionKey}' key.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    //ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddHttpClient();

        //Stripe Resilience:
        builder.Services.AddHttpClient("StripeClient")
            .AddResilienceHandler("StripeResiliencePolicy", pipelineBuilder =>
            {
                pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    ShouldHandle = args => ValueTask.FromResult(args.Outcome switch
                    {
                        { Exception: HttpRequestException } => true,
                        { Result.StatusCode: HttpStatusCode.TooManyRequests } => true,
                        { Result.StatusCode: >= HttpStatusCode.InternalServerError } => true,
                        _ => false
                    }),
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 5,
                    UseJitter = true,

                    OnRetry = args =>
                    {
                        var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("StripeResilience");

                        logger.LogWarning(
                            "Retrying request to Stripe... Attempt: {AttemptNumber}, Reason: {Reason}",
                            args.AttemptNumber + 1,
                            args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()
                        );

                        return ValueTask.CompletedTask;
                    }
                });
            });
    }
}
