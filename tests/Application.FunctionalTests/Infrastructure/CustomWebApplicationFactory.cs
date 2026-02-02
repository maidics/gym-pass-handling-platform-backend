using System.Data.Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FitPass.Application.Common.Interfaces;
using FitPass.Infrastructure.Data;
using FitPass.Infrastructure.Data.Interceptors;
using FitPass.Infrastructure.Stripe;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stripe;

namespace FitPass.Application.FunctionalTests.Infrastructure;

using static Testing.Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IContainer _stripeContainer = new ContainerBuilder()
        .WithImage("stripe/stripe-mock:latest")
        .WithPortBinding(12111, true)
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r =>
                    r.ForPort(12111)
                        .ForPath("/")
                        .ForStatusCode(System.Net.HttpStatusCode.Unauthorized)
                )
        )
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
        builder.UseSetting("ConnectionStrings:FitPassDb", _connectionString);

        builder.ConfigureTestServices(services =>
        {
            //IUser
            services
                .RemoveAll<IUser>()
                .AddTransient(_ =>
                {
                    var mock = new Mock<IUser>();
                    mock.SetupGet(x => x.Roles).Returns(GetCurrentUserRoles());
                    mock.SetupGet(x => x.Id).Returns(GetCurrentUserUserId());
                    return mock.Object;
                });

            //DB
            services.RemoveAll<ISaveChangesInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services
                .RemoveAll<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>(
                    (sp, options) =>
                    {
                        options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                        options.UseSqlServer(_connection);
                    }
                );

            //Stripe
            services.RemoveAll<IStripeClient>();

            services.AddSingleton<IStripeClient>(provider => new StripeClient(
                apiKey: provider.GetRequiredService<IConfiguration>()["Stripe:Key"],
                clientId: null,
                apiBase: $"http://{_stripeContainer.Hostname}:{_stripeContainer.GetMappedPublicPort(12111)}"
            ));
        });
    }

    public async Task InitialiseStripeAsync()
    {
        await _stripeContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _stripeContainer.StopAsync();
        await base.DisposeAsync();
    }
}
