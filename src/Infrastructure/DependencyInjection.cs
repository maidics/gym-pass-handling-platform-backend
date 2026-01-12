using System.Net;
using System.Security.Claims;
using System.Text;
using FitPass.Infrastructure.Data.Interceptors;
using FitPass.Infrastructure.Stripe.Services;
using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Data;
using FitPass.Infrastructure.Data.DbSeed;
using FitPass.Infrastructure.Data.Queries;
using FitPass.Infrastructure.Identity;
using FitPass.Infrastructure.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Stripe;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Settings;
using FitPass.Infrastructure.Email;
using FitPass.Infrastructure.Jwt;
using FitPass.Infrastructure.Stripe.Services.Webhook;
using FitPass.Infrastructure.Common;
using FitPass.Infrastructure.Localization;
using RazorLight;

namespace FitPass.Infrastructure;

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
            options.UseInMemoryDatabase("NSwagDb")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)); //stops exceptions from transaction uses for in memory db
            //options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, Identity.IdentityService>();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(ConfigurationSections.Jwt));
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        //Email
        builder.Services.AddSingleton<IRazorLightEngine>(_ =>
        {
            return new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(LocalEmailService).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        });
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(ConfigurationSections.Email));
        builder.Services.AddTransient<IEmailService, LocalEmailService>();

        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(ConfigurationSections.Stripe));

        builder.Services.AddScoped<IQueryService, QueryService>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Configuration.GetSection(ConfigurationSections.Jwt).Get<JwtSettings>();

                if (jwtSettings == null)
                {
                    throw new ArgumentException($"Jwt section not found with '{ConfigurationSections.Jwt}' key.");
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
        
        builder.Services.Configure<ClientAppSettings>(builder.Configuration.GetSection(ConfigurationSections.ClientApp));

        builder.Services
            .AddStripeServices(builder.Configuration)
            .AddStringLocalization(builder.Configuration);
    }

    extension(IServiceCollection services)
    {
        private IServiceCollection AddStripeServices(IConfiguration configuration)
        {
            var apiKey = configuration["Stripe:TestKey"];
            string stripeClientName = "StripeClient";
    
            //Stripe Resilience:
            services.AddHttpClient(stripeClientName)
                .AddResilienceHandler("StripeResiliencePolicy", pipelineBuilder =>
                {
                    pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                    {
                        ShouldHandle = async args => 
                        {
                            //network failures, DNS, connection dropped etc. 
                            if (args.Outcome.Exception is HttpRequestException)
                            {
                                return true;
                            }
    
                            if (args.Outcome.Result is { } response)
                            {
                                //rate limit
                                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                                {
                                    return true;
                                }
    
                                //server errors: 500, 502, 503, 504
                                if (response.StatusCode >= HttpStatusCode.InternalServerError)
                                {
                                    return true;
                                }
                                
                                if (response.StatusCode == HttpStatusCode.Conflict)
                                {
                                    return await StripeHttpResponseHelper.IsLockTimeoutAsync(response);
                                }
                            }
                            
                            return false;
                        },
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = 3,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(2)
                        
                        //should log warnings automatically
                        /*
                        OnRetry = args =>
                        {
                            var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                            var logger = loggerFactory.CreateLogger("StripeResilience");
    
                            logger.LogWarning(
                                "Retrying request to Stripe... Attempt: {AttemptNumber}, Reason: {Reason}",
                                args.AttemptNumber + 1,
                                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()
                            );
    
                            return ValueTask.CompletedTask;
                        }
                        */
                    });
                });
    
            services.AddSingleton<IStripeClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var resilientHttpClient = httpClientFactory.CreateClient(stripeClientName);
    
                var stripeAdapter = new SystemNetHttpClient(
                    httpClient: resilientHttpClient,
                    maxNetworkRetries: 0);
    
                return new StripeClient(httpClient: stripeAdapter, apiKey: apiKey);
            });
    
            services.AddScoped(provider => new AccountService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new AccountLinkService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new AccountLoginLinkService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new PaymentIntentService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new CustomerService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new PriceService(provider.GetRequiredService<IStripeClient>()));
            services.AddScoped(provider => new ProductService(provider.GetRequiredService<IStripeClient>()));
    
            services.AddScoped<IPaymentWebhookService, StripeWebhookService>();
            services.AddScoped<IPaymentTenantService, StripeConnectedAccountService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IPaymentPriceService, StripePriceService>();
            services.AddScoped<IPaymentProductService, StripeProductService>();

            return services;
        }
    
        private IServiceCollection AddStringLocalization(IConfiguration configuration)
        {
            services.Configure<CultureSettings>(configuration.GetSection(ConfigurationSections.Cultures));
            services.AddLocalization(); //SharedResource.cs marks the resx files directory
            services.AddTransient<ILocalizer, Localizer>();
    
            return services;
        }
    }
}
