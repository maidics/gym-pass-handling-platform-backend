using System.Text.Json;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize]
public record CreateGymCreationRequestCommand(
    string RequestDescription,
    PriorityLevel PriorityLevel,
    CreateGymDto CreateGymDTO
) : IRequest<Result>;

public class CreateGymCreationRequestCommandValidator : AbstractValidator<CreateGymCreationRequestCommand>
{
    public CreateGymCreationRequestCommandValidator()
    {
        RuleFor(v => v.CreateGymDTO.GymName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym name");

        RuleFor(v => v.CreateGymDTO.GymAddress).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Address, "Gym address");

        RuleFor(v => v.CreateGymDTO.GymStatus).NotEmptyWithMessage("Gym status");

        RuleFor(v => v.CreateGymDTO.GymTier).NotEmptyWithMessage("Gym tier");

        RuleFor(v => v.CreateGymDTO.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .EmailAddress().WithMessage("An escalation email address from a higher-level contact than the gym administrator is required.");
    }
}

public class CreateGymCreationRequestCommandHandler : IRequestHandler<CreateGymCreationRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymCreationRequestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Result> Handle(CreateGymCreationRequestCommand command, CancellationToken cancellationToken)
    {
        _user.ThrowIfIdNull();

        var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.Id == _user.Id, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        if (user.IsGymMember)
        {
            return Result.Failure(["A user who has purchased passes before cannot be nominated to Gym Administrator at Gym Creation. Please register a new account for this action."]);
        }

        if (user.GymStaffAssigment != null)
        {
            return Result.Failure(["You are already a Gym Administrator, you cannot be associated with two gyms."]);
        }

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

        user.Requests.Add(gymCreationRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}