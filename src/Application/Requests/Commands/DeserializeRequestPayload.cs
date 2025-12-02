using System.Text.Json;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
internal record DeserializeRequestPayloadCommand<TPayload>(Request Request) : IRequest<Result<TPayload>>;

internal class DeserializeRequestPayloadCommandHandler<TPayload>
    : IRequestHandler<DeserializeRequestPayloadCommand<TPayload>, Result<TPayload>>
{
    private readonly ILogger<DeserializeRequestPayloadCommandHandler<TPayload>> _logger;

    public DeserializeRequestPayloadCommandHandler(ILogger<DeserializeRequestPayloadCommandHandler<TPayload>> logger)
    {
        _logger = logger;
    }

    public Task<Result<TPayload>> Handle(DeserializeRequestPayloadCommand<TPayload> command, CancellationToken cancellationToken)
    {
        if (command.Request.Payload is null)
        {
            _logger.LogError("{Request} request has no payload.", command.Request);

            return Task.FromResult((Result<TPayload>)Result.InternalError("Request has no payload."));
        }

        TPayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<TPayload>(command.Request.Payload);

            if (payload is null)
            {
                return Task.FromResult((Result<TPayload>)Result.InternalError("Failed to deserialize request payload."));
            }
        } catch
        {
            return Task.FromResult((Result<TPayload>)Result.InternalError("Failed to deserialize request payload."));
        }

        return Task.FromResult((Result<TPayload>)Result<TPayload>.Success(payload));
    }
}
