using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Settings;
using FitPass.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace FitPass.Application.UserProfiles.Commands;

[Authorize]
public record UpdateMyUserProfileCommand(
    string FirstName,
    string LastName,
    string PreferredLanguage
) : IRequest<Result>;

public class UpdateMyUserProfileCommandValidator : AbstractValidator<UpdateMyUserProfileCommand>
{
    public UpdateMyUserProfileCommandValidator(ILocalizer localizer, IOptions<CultureSettings> options)
    {
        RuleFor(v => v.FirstName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FirstName), MaxLengths.Name);

        RuleFor(v => v.LastName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.LastName), MaxLengths.Name);

        RuleFor(v => v.PreferredLanguage).SupportedLanguageWithMessageLocalized(localizer, options.Value.SupportedCultures);
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
            .FirstOrDefaultAsync(x => x.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(profile, nameof(UserProfile), _user.Id);

        profile.FirstName = command.FirstName;
        profile.LastName = command.LastName;
        profile.PreferredLanguage = command.PreferredLanguage;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
