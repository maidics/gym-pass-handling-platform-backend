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
public record CreateGymAdminNominationRequestCommand
(
    string UserEmailToNominate,
    string RequestDescription,
    PriorityLevel RequestPriorityLevel,
    string EscalationEmail
) : IRequest<Result>;

public class CreateGymAdminNominationRequestCommandValidator : AbstractValidator<CreateGymAdminNominationRequestCommand>
{
    public CreateGymAdminNominationRequestCommandValidator()
    {
        RuleFor(v => v.UserEmailToNominate)
            .NotEmptyWithMessage("Email")
            .EmailAddress().WithMessage("A valid email address must be provided.");

        RuleFor(v => v.RequestDescription!).NotEmptyWithMaxLenghtAndMessage(MaxStringLengths.Description, "Request description");

        RuleFor(v => v.RequestPriorityLevel).NotEmptyWithMessage("Request priority level");

        RuleFor(v => v.EscalationEmail)
            .NotEmptyWithMessage("Escalation email")
            .EmailAddress().WithMessage("A valid email address must be provided.");
    }
}

public class CreateGymAdminNominationRequestCommandHandler : IRequestHandler<CreateGymAdminNominationRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateGymAdminNominationRequestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<Result> Handle(CreateGymAdminNominationRequestCommand command, CancellationToken cancellationToken)
    {
        _user.ThrowIfIdNull();

        var userToNominate = await _context
            .ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(au => au.Email == command.UserEmailToNominate, cancellationToken);

        Guard.Against.NotFound(command.UserEmailToNominate, userToNominate, "Email");

        if (_user.Roles!.Contains(Roles.GymAdministrator))
        {
            return Result.Failure([$"User associated with '{command.UserEmailToNominate}' is already a Gym Administrator."]);
        }

        if (userToNominate.UserGymMemberships != null && userToNominate.UserGymMemberships.Count > 0)
        {
            return Result.Failure([$"User associated with '{command.UserEmailToNominate}' has purchased passes before and cannot be nominated to Gym Administrator. Please register a new account for nomination."]);
        }

        var requester = await _context
            .ApplicationUsers
            .Include(au => au.GymStaffAssigment)
            .FirstOrDefaultAsync(au => au.Id == _user.Id, cancellationToken);

        var gym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == requester!.GymStaffAssigment!.GymId, cancellationToken);

        var request = new Request
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Gym Administrator Nomination",
            Description = command.RequestDescription,
            PriorityLevel = command.RequestPriorityLevel,
            Type = RequestType.GymAdminNomination,
            Payload = JsonSerializer.Serialize(new GymAdminNominationDto
            {
                GymId = gym!.Id,
                UserIdToNominate = userToNominate.Id,
                EscalationEmail = command.EscalationEmail
            })
        };

        await _context.Requests.AddAsync(request);

        requester!.Requests.Add(request);

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
