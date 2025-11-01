using System.Text.Json;
using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdminNominationRequestCommand
(
    string UserIdToNominate,
    string RequestDescription,
    PriorityLevel RequestPriorityLevel,
    string EscalationEmail
) : IRequest<Result>;

public class CreateGymAdminNominationRequestCommandValidator : AbstractValidator<CreateGymAdminNominationRequestCommand>
{
    public CreateGymAdminNominationRequestCommandValidator()
    {
        RuleFor(v => v.UserIdToNominate).NotEmptyWithMessage(nameof(CreateGymAdminNominationRequestCommand.UserIdToNominate));

        RuleFor(v => v.RequestDescription!)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymAdminNominationRequestCommand.RequestDescription), MaxStringLengths.Description);

        RuleFor(v => v.RequestPriorityLevel).IsInEnumWithMessage();

        RuleFor(v => v.EscalationEmail).ValidEmailAddress(nameof(CreateGymAdminNominationRequestCommand.EscalationEmail));
    }
}

public class CreateGymAdminNominationRequestCommandHandler : IRequestHandler<CreateGymAdminNominationRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<CreateGymAdminNominationRequestCommandHandler> _logger;
    private readonly IIdentityService _identityService;

    public CreateGymAdminNominationRequestCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        ILogger<CreateGymAdminNominationRequestCommandHandler> logger, 
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _identityService = identityService;
    }
    public async Task<Result> Handle(CreateGymAdminNominationRequestCommand command, CancellationToken cancellationToken)
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

        if (!await _identityService.DoesUserExist(command.UserIdToNominate))
        {
            throw new NotFoundException(command.UserIdToNominate, "User to nominate");
        }

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym Administrator Nomination",
            Description = command.RequestDescription,
            PriorityLevel = command.RequestPriorityLevel,
            Type = RequestType.GymAdminNomination,
            Payload = JsonSerializer.Serialize(new GymAdminNominationDto
            {
                GymId = requesterGymEmployment.GymId!,
                UserIdToNominate = command.UserIdToNominate,
                EscalationEmail = command.EscalationEmail
            })
        };

        await _context.Requests.AddAsync(request);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
