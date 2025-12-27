using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.UserProfiles.Commands;

[Authorize]
public record UpdateUserProfilePreferredLanguageCommand(string Language) : IRequest;

public class UpdateUserProfilePreferredLanguageCommandValidator : AbstractValidator<UpdateUserProfilePreferredLanguageCommand>
{
    public UpdateUserProfilePreferredLanguageCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Language)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.PreferredLanguage))
            .Must(localizer.IsSupported)
            .WithMessage(localizer.Get(nameof(SharedResource.LanguageIsNotSupported), string.Join(", ", localizer.SupportedCultures)));
    }
}

public class UpdateUserProfilePreferredLanguageCommandHandler : IRequestHandler<UpdateUserProfilePreferredLanguageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateUserProfilePreferredLanguageCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task Handle(UpdateUserProfilePreferredLanguageCommand command, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(profile, nameof(UserProfile), _user.Id);

        profile.PreferredLanguage = command.Language;

        await _context.SaveChangesAsync();
    }
}
