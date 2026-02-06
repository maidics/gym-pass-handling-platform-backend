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

[Authorize(Roles = Roles.PendingGymEmployee)]
public record CreateGymCreationRequestCommand(
    string Title,
    string Description,
    PriorityLevel PriorityLevel,
    CreateGymDto CreateGymDto
) : IRequest<Result<RequestDto>>;

public class CreateGymCreationRequestCommandValidator
    : AbstractValidator<CreateGymCreationRequestCommand>
{
    public CreateGymCreationRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(x => x.Title)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Title),
                MaxLengths.Title
            );

        RuleFor(v => v.Description)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Description),
                MaxLengths.Description
            );

        RuleFor(v => v.CreateGymDto.Name)
            .NotEmptyWithMaxLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Name),
                MaxLengths.Name
            );

        RuleFor(v => v.CreateGymDto.Address)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Address));

        RuleFor(v => v.CreateGymDto.Status)
            .Must(status => status != GymStatus.Suspended)
            .WithMessage(
                localizer.GetWithParamsLocalized(
                    nameof(SharedResource.ValueIsInvalid),
                    nameof(SharedResource.GymStatus)
                )
            );

        RuleFor(v => v.CreateGymDto.SupervisorEmail).EmailAddressWithMessageLocalized(localizer);
    }
}

public class CreateGymCreationRequestCommandHandler
    : IRequestHandler<CreateGymCreationRequestCommand, Result<RequestDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public CreateGymCreationRequestCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer
    )
    {
        _identityService = identityService;
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result<RequestDto>> Handle(
        CreateGymCreationRequestCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.RequiresEmailConfirmation))
            );
        }

        var ongoingRequests = await _context
            .Requests.Where(r =>
                r.CreatedBy == _user.Id
                && r.Status == RequestStatus.Submitted
                && r.Type == RequestType.GymCreation
            )
            .ToListAsync(cancellationToken);

        if (ongoingRequests.Count > 0)
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.AlreadyHaveOnGoingRequestOfThisType))
            );
        }

        var request = new Request
        {
            Title = command.Title,
            Description = command.Description,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymCreation,
            Payload = JsonSerializer.Serialize(
                command.CreateGymDto,
                JsonDefaults.SerializerOptions
            ),
        };

        await _context.Requests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(request.MapToDto());
    }
}
