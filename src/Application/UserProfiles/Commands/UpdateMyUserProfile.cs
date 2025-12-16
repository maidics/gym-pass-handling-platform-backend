using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.UserProfiles.Commands;

[Authorize]
public record UpdateMyUserProfileCommand(
    string FirstName,
    string LastName
) : IRequest<Result>;

public class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.FirstName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FirstName), MaxLength.Name);

        RuleFor(v => v.LastName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.LastName), MaxLength.Name);
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
