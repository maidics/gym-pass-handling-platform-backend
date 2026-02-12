using System.Text.Json;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Common.Settings;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdminPromotionRequestCommand(
    string Title,
    string PendingGymEmployeeEmail,
    string Description,
    PriorityLevel PriorityLevel,
    string SupervisorEmail
) : IRequest<Result>;

public class CreateGymAdminPromotionRequestCommandValidator
    : AbstractValidator<CreateGymAdminPromotionRequestCommand>
{
    public CreateGymAdminPromotionRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(x => x.Title)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Title),
                MaxLengths.Title
            );

        RuleFor(v => v.PendingGymEmployeeEmail).EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.Description!)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Description),
                MaxLengths.Description
            );

        RuleFor(v => v.SupervisorEmail).EmailAddressWithMessageLocalized(localizer);
    }
}

public class CreateGymAdminPromotionRequestCommandHandler
    : IRequestHandler<CreateGymAdminPromotionRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILocalizer _localizer;

    public CreateGymAdminPromotionRequestCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        CreateGymAdminPromotionRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        var gymEmployment = await _context
            .GymEmployments.AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(
            gymEmployment,
            nameof(GymEmployment),
            _user.Id
        );

        if (!await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.RequiresEmailConfirmation))
            );
        }

        var pendingGymEmployeeId = await _identityService.GetUserIdByEmailAsync(
            command.PendingGymEmployeeEmail
        );

        if (
            pendingGymEmployeeId is null
            || !await _identityService.IsInRoleAsync(pendingGymEmployeeId, Roles.PendingGymEmployee)
        )
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        var request = new Request
        {
            Title = command.Title,
            Description = command.Description,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymAdminPromotion,
            Payload = JsonSerializer.Serialize(
                new GymAdminPromotionDto
                {
                    GymId = gymEmployment.GymId!,
                    PendingGymEmployeeEmail = command.PendingGymEmployeeEmail,
                    SupervisorEmail = command.SupervisorEmail,
                },
                JsonDefaults.SerializerOptions
            ),
        };

        await _context.Requests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
