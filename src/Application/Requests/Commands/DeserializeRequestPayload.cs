using System.Text.Json;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
internal record DeserializeRequestPayloadCommand<TPayload>(Request Request) : IRequest<Result<TPayload, RequestStatus>>;

internal class DeserializeRequestPayloadCommandHandler<TPayload>
    : IRequestHandler<DeserializeRequestPayloadCommand<TPayload>, Result<TPayload, RequestStatus>>
{
    private readonly ILogger<DeserializeRequestPayloadCommandHandler<TPayload>> _logger;

    public DeserializeRequestPayloadCommandHandler(ILogger<DeserializeRequestPayloadCommandHandler<TPayload>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TPayload, RequestStatus>> Handle(DeserializeRequestPayloadCommand<TPayload> command, CancellationToken cancellationToken)
    {
        if (command.Request.Payload is null)
        {
            _logger.LogError("{Request} request has no payload.", command.Request);

            return Result<TPayload, RequestStatus>.Failure(["Request has no payload."], RequestStatus.PayloadWasNull);
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

                return Result<TPayload, RequestStatus>.Failure(["Failed to deserialize request payload."], RequestStatus.PayloadFailedToSerialize);
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

            return Result<TPayload, RequestStatus>.Failure(["Failed to deserialize request payload."], RequestStatus.PayloadFailedToSerialize, ex);
        }

        return Result<TPayload, RequestStatus>.Success(payload);
    }
}
