using System.Text.Json;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

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

        RuleFor(v => v.CreateGymDTO.GymAdminEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Email address")
            .EmailAddress().WithMessage("An email address is required for the gym administrator account.");

        RuleFor(v => v.CreateGymDTO.GymAdminFirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym Administrator's first name");

        RuleFor(v => v.CreateGymDTO.GymAdminLastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym Administrator's last name");

        RuleFor(v => v.CreateGymDTO.GymAdminPassword)
            .NotEmptyWithMessage("Password")
            .StrongPassword();

        RuleFor(v => v.CreateGymDTO.GymAdminPasswordConfirm)
            .NotEmptyWithMessage("Password confirmation")
            .Equal(v => v.CreateGymDTO.GymAdminPassword).WithMessage("Gym Administrator's password and password confirmation must match.");

        RuleFor(v => v.CreateGymDTO.EscalationEmail)
            .NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Email, "Escalation email")
            .EmailAddress().WithMessage("An escalation email address from a higher-level contact than the gym administrator is required.");
    }
}

public class CreateGymCreationRequestCommandHandler : IRequestHandler<CreateGymCreationRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CreateGymCreationRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CreateGymCreationRequestCommand request, CancellationToken cancellationToken)
    {
        var gymCreationRequest = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"'{request.CreateGymDTO.GymName}' gym creation",
            Description = request.RequestDescription,
            PriorityLevel = request.PriorityLevel,
            Type = RequestType.GymCreation,
            Payload = JsonSerializer.Serialize(request.CreateGymDTO),
        };

        await _context.Requests.AddAsync(gymCreationRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}