using System.Text.Json;
using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Application.Common.Models;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdminPromotionRequestCommand
(
    string UserIdToPromote,
    string RequestDescription,
    PriorityLevel RequestPriorityLevel,
    string EscalationEmail
) : IRequest<Result<RequestDto>>;

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

public class CreateGymAdminPromotionRequestCommandHandler : IRequestHandler<CreateGymAdminPromotionRequestCommand, Result<RequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public CreateGymAdminPromotionRequestCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    public async Task<Result<RequestDto>> Handle(CreateGymAdminPromotionRequestCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        if (!await _identityService.DoesUserExist(command.UserIdToPromote))
        {
            return Result.NotFound("User to promote");
        }

        if (!await _identityService.IsInRoleAsync(command.UserIdToPromote, Roles.PendingGymEmployee))
        {
            return Result.BusinessRuleViolation("User is not in PendingGymEmployee role.");
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
                GymId = gymEmployment.GymId!,
                UserIdToNominate = command.UserIdToPromote,
                EscalationEmail = command.EscalationEmail
            })
        };

        await _context.Requests.AddAsync(request);
        await _context.SaveChangesAsync();

        return Result.Success(request.MapToDto());
    }
}
