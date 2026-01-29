using System.Text.Json;
using FitPass.Application.Requests.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdminPromotionRequestCommand
(
    string PendingGymEmployeeEmail,
    string Description,
    PriorityLevel PriorityLevel,
    string SupervisorEmail
) : IRequest<Result>;

public class CreateGymAdminPromotionRequestCommandValidator : AbstractValidator<CreateGymAdminPromotionRequestCommand>
{
    public CreateGymAdminPromotionRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.PendingGymEmployeeEmail)
            .EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.Description!)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.Description), MaxLengths.Description);

        RuleFor(v => v.SupervisorEmail)
            .EmailAddressWithMessageLocalized(localizer);
    }
}

public class CreateGymAdminPromotionRequestCommandHandler : IRequestHandler<CreateGymAdminPromotionRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILocalizer _localizer;

    public CreateGymAdminPromotionRequestCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _localizer = localizer;
    }
    public async Task<Result> Handle(CreateGymAdminPromotionRequestCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var pendingGymEmployeeId = await _identityService.GetUserIdByEmailAsync(command.PendingGymEmployeeEmail);

        if (pendingGymEmployeeId is null || !await _identityService.IsInRoleAsync(pendingGymEmployeeId, Roles.PendingGymEmployee))
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym Administrator Nomination",
            Description = command.Description,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymAdminPromotion,
            Payload = JsonSerializer.Serialize(new GymAdminPromotionDto
            {
                GymId = gymEmployment.GymId!,
                PendingGymEmployeeEmail = pendingGymEmployeeId,
                SupervisorEmail = command.SupervisorEmail
            })
        };

        await _context.Requests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
