using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
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
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<UpdateMyUserProfileCommandHandler> _logger;

    public UpdateMyUserProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<UpdateMyUserProfileCommandHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
    
    public async Task Handle(UpdateMyUserProfileCommand command, CancellationToken cancellationToken) //TODO: test if this saves
    {
        var userProfile = await _context
            .UserProfiles
            .FindAsync(_user.Id!);

        if (userProfile == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(userProfile));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(userProfile)));
        }

        userProfile.FirstName = command.FirstName;
        userProfile.LastName = command.LastName;

        await _context.SaveChangesAsync();
    }
}
