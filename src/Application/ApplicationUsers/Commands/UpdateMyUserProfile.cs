using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize]
public record UpdateMyUserProfileCommand(
    string FirstName,
    string LastName
) : IRequest;

public class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyUserProfileCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyUserProfileCommand.LastName), MaxStringLengths.Name);
    }
}

public class UpdateMyUserProfileCommandHandler : IRequestHandler<UpdateMyUserProfileCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateMyUserProfileCommandHandler> _logger;

    public UpdateMyUserProfileCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateMyUserProfileCommandHandler> logger)
    {
        _identityService = identityService;
        _context = context;
        _user = user;
        _logger = logger;
    }
    
    public async Task Handle(UpdateMyUserProfileCommand command, CancellationToken cancellationToken) //TODO: test if this saves
    {
        var result = await _identityService.UpdateUserFirstAndLastName(_user.Id!, command.FirstName, command.LastName);

        if (result.IsUserNotFoundFailure())
        {
            LogCriticalMessages.AuthenticatedUserNotFound(_logger, _user.Roles, _user.Id, null);
            throw new Exception(ErrorMessages.AuthenticatedUserNotFound());
        }

        if (!result.Succeeded)
        {
            LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.UpdateUserFirstAndLastName), _user.Roles?[0], _user.Id, result);
            throw new Exception("Failed to update user's first and last name.");
        }
    }
}
