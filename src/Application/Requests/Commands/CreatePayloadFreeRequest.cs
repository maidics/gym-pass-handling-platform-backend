using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(
    Roles = $"{Roles.User},{Roles.PendingGymEmployee},{Roles.GymStaff},{Roles.GymAdministrator}"
)]
public record CreatePayloadFreeRequestCommand(
    string Title,
    string Description,
    PriorityLevel PriorityLevel,
    RequestType RequestType
) : IRequest<Result>;

public class CreatePayloadFreeRequestCommandValidator
    : AbstractValidator<CreatePayloadFreeRequestCommand>
{
    public CreatePayloadFreeRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Title).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Title));

        RuleFor(v => v.Description)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Description));

        RuleFor(v => v.RequestType)
            .NotEmpty()
            .WithMessage(
                localizer.GetPropertyOfEntityIsRequired(
                    nameof(SharedResource.Type),
                    nameof(SharedResource.Request)
                )
            );

        RuleFor(v => v.RequestType)
            .Must(v => v != RequestType.GymAdminPromotion && v != RequestType.GymCreation)
            .WithMessage(localizer.Get(nameof(SharedResource.PayloadFreeRequestTypeRules)));
    }
}

public class CreatePayloadFreeRequestCommandHandler
    : IRequestHandler<CreatePayloadFreeRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public CreatePayloadFreeRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user,
        ILocalizer localizer
    )
    {
        _context = context;
        _identityService = identityService;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        CreatePayloadFreeRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.RequiresEmailConfirmation))
            );
        }

        var request = new Request
        {
            Title = command.Title,
            Description = command.Description,
            PriorityLevel = command.PriorityLevel,
            Type = command.RequestType,
            Payload = null,
        };

        await _context.Requests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
