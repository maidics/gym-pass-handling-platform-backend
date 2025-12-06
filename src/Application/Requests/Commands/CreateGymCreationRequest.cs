using System.Text.Json;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.PendingGymEmployee)]
public record CreateGymCreationRequestCommand(
    string RequestDescription,
    PriorityLevel PriorityLevel,
    CreateGymDto CreateGymDto
) : IRequest<Result<RequestDto>>;

public class CreateGymCreationRequestCommandValidator : AbstractValidator<CreateGymCreationRequestCommand>
{
    public CreateGymCreationRequestCommandValidator()
    {
        RuleFor(v => v.RequestDescription)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymCreationRequestCommand.RequestDescription), MaxStringLengths.Description);

        RuleFor(v => v.CreateGymDto.GymName)
            .NotEmptyWithMaxLenghtAndMessage(nameof(CreateGymCreationRequestCommand.CreateGymDto.GymName), MaxStringLengths.Description);

        RuleFor(v => v.CreateGymDto.GymAddress)
            .NotEmptyWithMessage(nameof(CreateGymCreationRequestCommand.CreateGymDto.GymAddress));

        RuleFor(v => v.CreateGymDto.GymStatus)
            .Must(status => status != GymStatus.Suspended)
            .WithMessage("Gym status cannot be Suspended for a new gym.");

        RuleFor(v => v.CreateGymDto.EscalationEmail)
            .ValidEmailAddress(nameof(CreateGymCreationRequestCommand.CreateGymDto.EscalationEmail));
    }
}

public class CreateGymCreationRequestCommandHandler : IRequestHandler<CreateGymCreationRequestCommand, Result<RequestDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymCreationRequestCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IUser user)
    {
        _identityService = identityService;
        _context = context;
        _user = user;
    }

    public async Task<Result<RequestDto>> Handle(CreateGymCreationRequestCommand command, CancellationToken cancellationToken)
    {
        if (!await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation("You must confirm your email before this action.");
        }

        var ongoingRequests = await _context.Requests
            .Where(r => r.CreatedBy == _user.Id && r.Status == RequestStatus.Submitted && r.Type == RequestType.GymCreation)
            .ToListAsync();

        if (ongoingRequests.Count > 0)
        {
            return Result.BusinessRuleViolation("You already have an ongoing gym creation request.");
        }

        //TODO: add validations here

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym creation",
            Description = command.RequestDescription,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymCreation,
            Payload = JsonSerializer.Serialize(command.CreateGymDto),
        };

        await _context.Requests.AddAsync(request);

        await _context.SaveChangesAsync();

        return Result.Success(request.MapToDto());
    }
}
