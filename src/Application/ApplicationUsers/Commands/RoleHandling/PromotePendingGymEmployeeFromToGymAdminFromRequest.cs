using System.Text.Json;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.Commands;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands.RoleHandling;


[Authorize(Roles = Roles.AppAdministrator)]
public record PromotePendingGymEmployeeFromToGymAdminFromRequestCommand(string RequestId) : IRequest;

public class PromotePendingGymEmployeeFromToGymAdminFromRequestCommandValidator : AbstractValidator<PromotePendingGymEmployeeFromToGymAdminFromRequestCommand>
{
    public PromotePendingGymEmployeeFromToGymAdminFromRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(PromotePendingGymEmployeeFromToGymAdminFromRequestCommand.RequestId))
    }
}

public class PromotePendingGymEmployeeFromToGymAdminFromRequestCommandHandler : IRequestHandler<PromotePendingGymEmployeeFromToGymAdminFromRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;
    private readonly ILogger<PromotePendingGymEmployeeFromToGymAdminFromRequestCommand> _logger;
    private readonly IIdentityService _identityService;

    public PromotePendingGymEmployeeFromToGymAdminFromRequestCommandHandler(
        IApplicationDbContext context,
        ISender sender,
        ILogger<PromotePendingGymEmployeeFromToGymAdminFromRequestCommand> logger,
        IIdentityService identityService)
    {
        _context = context;
        _sender = sender;
        _logger = logger;
        _identityService = identityService;
    }

    public async Task Handle(PromotePendingGymEmployeeFromToGymAdminFromRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.FindAsync(command.RequestId);

        Guard.Against.NotFound(command.RequestId, request);

        if (request.Status != RequestStatus.Submitted)
        {
            throw new ForbiddenAccessException();
        }

        if (request.Type != RequestType.GymCreation)
        {
            throw new BadRequestException("Request is not of GymCreation type.");
        }

        var deserializationResult = await _sender.Send(new DeserializeRequestPayloadCommand<GymAdminPromotionDto>(request));

        if (!deserializationResult.Succeeded)
        {
            request.Status = deserializationResult.FailureType;

            await _context.SaveChangesAsync();

            throw new ArgumentException("Failed to deserialize payload.");
        }

        throw new NotImplementedException();
    }
}
