using System.Text.Json;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.PendingGymAdministrator)]
public record CreateGymCreationRequestCommand(
    string RequestDescription,
    PriorityLevel PriorityLevel,
    CreateGymDto CreateGymDTO
) : IRequest;

public class CreateGymCreationRequestCommandValidator : AbstractValidator<CreateGymCreationRequestCommand>
{
    public CreateGymCreationRequestCommandValidator()
    {
        RuleFor(v => v.CreateGymDTO.GymName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym name");

        RuleFor(v => v.CreateGymDTO.GymAddress).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Address, "Gym address");

        RuleFor(v => v.CreateGymDTO.GymStatus).IsInEnumWithMessage("Gym status");

        RuleFor(v => v.CreateGymDTO.GymTier).IsInEnumWithMessage("Gym tier");

        RuleFor(v => v.CreateGymDTO.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .EmailAddress().WithMessage("An escalation email address from a higher-level contact than the gym administrator is required.");
    }
}

public class CreateGymCreationRequestCommandHandler : IRequestHandler<CreateGymCreationRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymCreationRequestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CreateGymCreationRequestCommand command, CancellationToken cancellationToken)
    {
        var gymCreationRequest = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"'{command.CreateGymDTO.GymName}' gym creation",
            Description = command.RequestDescription,
            PriorityLevel = command.PriorityLevel,
            Type = RequestType.GymCreation,
            Payload = JsonSerializer.Serialize(command.CreateGymDTO),
        };

        await _context.Requests.AddAsync(gymCreationRequest, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
