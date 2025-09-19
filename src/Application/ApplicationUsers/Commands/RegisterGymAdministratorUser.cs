using System.Text.Json;
using Fitpass.Application.Requests.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace Fitpass.Application.ApplicationUsers.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymAdministratorUserCommand(string RequestId) : IRequest<Result>;

public class RegisterGymAdministratorUserCommandValidator : AbstractValidator<RegisterGymAdministratorUserCommand>
{
    public RegisterGymAdministratorUserCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage("Request id");
    }
}

public class RegisterGymAdministratorUserCommandHandler : IRequestHandler<RegisterGymAdministratorUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public RegisterGymAdministratorUserCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(RegisterGymAdministratorUserCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

        if (request == null)
        {
            return Result.Failure(["Request not found."]);
        }

        if (request.Type != RequestType.GymAdministratorAccountCreation)
        {
            return Result.Failure(["Request is not of Gym administrator account creation type."]);
        }

        var requestDto = JsonSerializer.Deserialize<CreateGymAdministratorUserDto>(request.Payload!);

        if (requestDto == null)
        {
            return Result.Failure(["Unable serialize gym administrator account creation details."]);
        }

        var gym = await _context.Gyms.AsNoTracking().FirstOrDefaultAsync(g => g.Id == requestDto.GymId, cancellationToken);

        if (gym == null)
        {
            return Result.Failure(["Specified gym is not found from request details."]);
        }

        var result = await _identityService.CreateGymManagementUserAsync
        (
            requestDto.GymAdminEmail,
            requestDto.GymAdminPassword,
            requestDto.GymAdminFirstName,
            requestDto.GymAdminLastName,
            Roles.GymAdministrator,
            gym,
            requestDto.EscalationEmail
        );

        return Result.Success();
    }
}