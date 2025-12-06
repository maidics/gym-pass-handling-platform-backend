using System.Data.Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Data;
using FitPass.Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stripe;

namespace FitPass.Application.FunctionalTests;

using static Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IContainer _stripeContainer = new ContainerBuilder()
        .WithImage("stripe/stripe-mock:latest")
        .WithPortBinding(12111, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(s => s
            .ForPort(12111)
            .ForPath("healthcheck")
        ))
        .Build();

    private readonly DbConnection _connection;
    private readonly string _connectionString;

    public CustomWebApplicationFactory(DbConnection connection, string connectionString)
    {
        _connection = connection;
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _stripeContainer.StartAsync().Wait();

        var stripeUrl = $"http://{_stripeContainer.Hostname}:{_stripeContainer.GetMappedPublicPort(12111)}";

        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<IStripeClient>();

            services.AddSingleton<IStripeClient>(provider =>
            
                new StripeClient(
                    apiKey: "sk_test_docker_api_key",
                    clientId: null,
                    apiBase: stripeUrl)
            );
        });

        builder
            .UseEnvironment("Testing")
            .UseSetting("ConnectionStrings:FitPassDb", _connectionString);

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(provider =>
                {
                    var mock = new Mock<IUser>();
                    mock.SetupGet(x => x.Roles).Returns(GetCurrentUserRoles());
                    mock.SetupGet(x => x.Id).Returns(GetCurrentUserUserId());
                    return mock.Object;
                });

            services.RemoveAll<ISaveChangesInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services
                .RemoveAll<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>((sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseSqlServer(_connection);
                });
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("./appsettings.Test.json", optional: false);
        });
    }
}
