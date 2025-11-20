using System.Text.Json;
using FitPass.Application.Common.Logging;
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

    public async Task<Result<TPayload>> Handle(DeserializeRequestPayloadCommand<TPayload> command, CancellationToken cancellationToken)
    {
        if (command.Request.Payload is null)
        {
            _logger.LogError("{Request} request has no payload.", command.Request);

            return Result<TPayload>.Failure(["Request has no payload."], ResultTypes.InternalError);
        }

        TPayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<TPayload>(command.Request.Payload);

            if (payload is null)
            {
                LogErrorMessages.JsonSerilaizationFailure(
                    _logger,
                    "Deserialization",
                    nameof(JsonSerializer.Deserialize),
                    nameof(Request),
                    command.Request.CreatedBy,
                    command.Request.Payload,
                    null);

                return Result<TPayload>.Failure(["Failed to deserialize request payload."], ResultTypes.InternalError);
            }
        } catch (Exception ex)
        {
            LogErrorMessages.JsonSerilaizationFailure(
                    _logger,
                    "Deserialization",
                    nameof(JsonSerializer.Deserialize),
                    nameof(Request),
                    command.Request.CreatedBy,
                    command.Request.Payload,
                    ex);

            return Result<TPayload>.Failure(["Failed to deserialize request payload."], ResultTypes.InternalError);
        }

        return Result<TPayload>.Success(payload);
    }
}
