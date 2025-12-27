using System.Net;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Shouldly;

namespace FitPass.Infrastructure.IntegrationTests.ResilienceTests;

/*
public class StripeResilienceTests
{

    [TestCase(HttpStatusCode.TooManyRequests, 4, "ExternalServiceUnavailable")]
    [TestCase(HttpStatusCode.TooManyRequests, 2, "Success")]
    [TestCase(HttpStatusCode.PaymentRequired, 4, "PaymentRequired")]
    [TestCase(HttpStatusCode.PaymentRequired, 1, "Success")]
    [TestCase(HttpStatusCode.NotFound, 4, "InternalError")]
    [TestCase(HttpStatusCode.NotFound, 0, "Success")]
    public async Task ServiceShouldReturnCorrectResultTypeForHttpErrors(HttpStatusCode statusCode, int tryCount, string resultType)
    {
        var mockHttpMessageHandler = new MockHttpMessageHandler(statusCode, tryCount);
        
        var host = GetHost(mockHttpMessageHandler);

        using var scope = host.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IPaymentTenantService>();

        var result = await service.CreateTenantAccount(
            "test_gymId",
            "test@localhost",
            "Test Business");
        
        result.Type.ShouldBe(GetResult(resultType).Type);
        
        mockHttpMessageHandler.CallCount.ShouldBe(tryCount);
    }

    private static IHost GetHost(MockHttpMessageHandler mockHttpMessageHandler)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddJsonFile(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src", "Web", "appsettings.Test.json"),
            optional: false);
        
        builder.AddInfrastructureServices();

        builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(httpBuilder =>
            {
                if (httpBuilder.Name == "StripeClient")
                {
                    httpBuilder.PrimaryHandler = mockHttpMessageHandler;
                }
            });
        });

        return builder.Build();
    }

    private static Result GetResult(string type)
    {
        return type switch
        {
            "Success" => Result.Success(),
            "ExternalServiceUnavailable" => new ResultFailure(ResultTypes.ExternalServiceUnavailable,
                "External service unavailable", []),
            "PaymentRequired" => new ResultFailure(ResultTypes.PaymentRequired, "Payment required", []),
            _ => new ResultFailure(ResultTypes.InternalError, "Internal error", [])
        };
    }
}
*/
