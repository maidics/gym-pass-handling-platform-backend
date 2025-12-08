using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.UserProfiles.Commands;

[Authorize]
public record UpdateMyUserProfileCommand(
    string FirstName,
    string LastName
) : IRequest<Result>;

public class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyUserProfileCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(UpdateMyUserProfileCommand.LastName), MaxStringLengths.Name);
    }
}

public class UpdateMyUserProfileCommandHandler : IRequestHandler<UpdateMyUserProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateMyUserProfileCommandHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<Result> Handle(UpdateMyUserProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = await _context
            .UserProfiles
            .FindAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(profile, nameof(UserProfile), _user.Id);

        profile.FirstName = command.FirstName;
        profile.LastName = command.LastName;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
