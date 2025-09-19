using System.Text.Json;
using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymAdministratorUserRequestCommand
(
    string GymId,
    string RequestDescription,
    PriorityLevel RequestPriorityLevel,
    string GymAdminFirstName,
    string GymAdminLastName,
    string GymAdminEmail,
    string GymAdminPassword,
    string GymAdminPasswordConfirm,
    string EscalationEmail
) : IRequest<Result>;

public class CreateGymAdministratorUserRequestCommandValidator : AbstractValidator<CreateGymAdministratorUserRequestCommand>
{
    public CreateGymAdministratorUserRequestCommandValidator()
    {
        RuleFor(v => v.GymId).NotEmptyWithMessage("Gym id");

        RuleFor(v => v.RequestDescription!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Description, "Request description");

        RuleFor(v => v.RequestPriorityLevel).NotEmptyWithMessage("Request priority level");

        RuleFor(v => v.GymAdminFirstName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym admin first name");

        RuleFor(v => v.GymAdminLastName).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Name, "Gym admin last name");

        RuleFor(v => v.GymAdminEmail)
            .NotEmptyWithMessage("Gym admin email")
            .EmailAddress().WithMessage("A valid email address must be provided.");

        RuleFor(v => v.GymAdminPassword)
            .NotEmptyWithMessage("Gym admin password")
            .StrongPassword();

        RuleFor(v => v.GymAdminPasswordConfirm).Matches(v => v.GymAdminPassword).WithMessage("Password and password confirmation must match.");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMessage("Escalation email")
            .EmailAddress().WithMessage("A valid email address must be provided.");
    }
}

public class CreateGymAdministratorUserRequestCommandHandler : IRequestHandler<CreateGymAdministratorUserRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CreateGymAdministratorUserRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Result> Handle(CreateGymAdministratorUserRequestCommand command, CancellationToken cancellationToken)
    {
        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == command.GymId, cancellationToken);

        if (gym == null)
        {
            return Result.Failure(["Gym not found"]);
        }

        var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == command.GymAdminEmail, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure(["This email is already in use"]);
        }

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym Administrator Account Creation",
            Description = command.RequestDescription,
            PriorityLevel = command.RequestPriorityLevel,
            Type = RequestType.GymAdministratorAccountCreation,
            Payload = JsonSerializer.Serialize(new CreateGymAdministratorUserDto
            {
                GymId = command.GymId,
                GymAdminFirstName = command.GymAdminFirstName,
                GymAdminLastName = command.GymAdminLastName,
                GymAdminEmail = command.GymAdminEmail,
                GymAdminPassword = command.GymAdminPassword,
                EscalationEmail = command.EscalationEmail
            })
        };

        await _context.Requests.AddAsync(request);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}