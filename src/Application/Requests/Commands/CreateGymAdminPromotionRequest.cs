using System.Text.Json;
using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;
using FitPass.Application.Common.Exceptions;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdminPromotionRequestCommand
(
    string UserIdToPromote,
    string RequestDescription,
    PriorityLevel RequestPriorityLevel,
    string EscalationEmail
) : IRequest;

public class CreateGymAdminPromotionRequestCommandValidator : AbstractValidator<CreateGymAdminPromotionRequestCommand>
{
    public CreateGymAdminPromotionRequestCommandValidator()
    {
        RuleFor(v => v.UserIdToPromote).NotEmptyWithMessage(nameof(CreateGymAdminPromotionRequestCommand.UserIdToPromote));

        RuleFor(v => v.RequestDescription!)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymAdminPromotionRequestCommand.RequestDescription), MaxStringLengths.Description);

        RuleFor(v => v.EscalationEmail).ValidEmailAddress(nameof(CreateGymAdminPromotionRequestCommand.EscalationEmail));
    }
}

public class CreateGymAdminPromotionRequestCommandHandler : IRequestHandler<CreateGymAdminPromotionRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<CreateGymAdminPromotionRequestCommandHandler> _logger;
    private readonly IIdentityService _identityService;

    public CreateGymAdminPromotionRequestCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        ILogger<CreateGymAdminPromotionRequestCommandHandler> logger, 
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _identityService = identityService;
    }
    public async Task Handle(CreateGymAdminPromotionRequestCommand command, CancellationToken cancellationToken)
    {
        var requesterGymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId != null && ge.ApplicationUserId == _user.Id);

        if (requesterGymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        if (!await _identityService.DoesUserExist(command.UserIdToPromote))
        {
            throw new NotFoundException(command.UserIdToPromote, "User to nominate");
        }

        if (!await _identityService.IsInRoleAsync(command.UserIdToPromote, Roles.PendingGymEmployee))
        {
            throw new BadRequestException("User is not in PendingGymEmployee role.");
        }

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym Administrator Nomination",
            Description = command.RequestDescription,
            PriorityLevel = command.RequestPriorityLevel,
            Type = RequestType.GymAdminPromotion,
            Payload = JsonSerializer.Serialize(new GymAdminPromotionDto
            {
                GymId = requesterGymEmployment.GymId!,
                UserIdToNominate = command.UserIdToPromote,
                EscalationEmail = command.EscalationEmail
            })
        };

        await _context.Requests.AddAsync(request);
        await _context.SaveChangesAsync();
    }
}
