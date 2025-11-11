using FitPass.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FitPass.Application.FunctionalTests;

public partial class Testing
{
    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static async Task ShouldThrowIfParametersAreInvalid(IBaseRequest request)
    {
        await Should.ThrowAsync<ValidationException>(SendAsync(request));
    }

    public static async Task ShouldThrowIfNotFound(IBaseRequest request)
    {
        await Should.ThrowAsync<NotFoundException>(SendAsync(request));
    }
}
