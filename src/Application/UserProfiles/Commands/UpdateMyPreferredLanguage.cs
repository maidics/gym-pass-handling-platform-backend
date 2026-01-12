using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Common.Settings;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FitPass.Application.UserProfiles.Commands;

[Authorize]
public record UpdateMyPreferredLanguageCommand(string NewLanguage) : IRequest;

public class UpdateMyPreferredLanguageCommandValidator : AbstractValidator<UpdateMyPreferredLanguageCommand>
{
    public UpdateMyPreferredLanguageCommandValidator(ILocalizer localizer, IOptions<CultureSettings> options)
    {
        RuleFor(v => v.NewLanguage).SupportedLanguageWithMessageLocalized(localizer, options.Value.SupportedCultures);
    }
}

public class UpdateMyPreferredLanguageCommandHandler : IRequestHandler<UpdateMyPreferredLanguageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateMyPreferredLanguageCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task Handle(UpdateMyPreferredLanguageCommand command, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(profile, nameof(UserProfile), _user.Id);

        profile.PreferredLanguage = command.NewLanguage;

        await _context.SaveChangesAsync();
    }
}
