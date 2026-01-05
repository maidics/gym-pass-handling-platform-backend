using FitPass.Application.Common.EmailModels.Users;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Common.Settings;
using FitPass.Application.Common.Resources;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Options;

namespace FitPass.Application.Users.Commands.Emails;

[Authorize]
public record SendEmailConfirmationEmailCommand : IRequest<Result>;

public class SendEmailConfirmationEmailCommandHandler : IRequestHandler<SendEmailConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILocalizer _localizer;

    public SendEmailConfirmationEmailCommandHandler(
        IIdentityService identityService,
        IUser user,
        IOptions<ClientAppSettings> options,
        IApplicationDbContext context,
        IEmailService emailService,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _user = user;
        _clientAppSettings = options.Value;
        _context = context;
        _emailService = emailService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.EmailIsAlreadyConfirmed)));
        }

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(_user.Id!);

        Guard.Against.Null(token, "Email confirmation token", "Failed to generate email confirmation token.");

        var email = await _identityService.GetEmailByIdAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(email, "Email", _user.Id);

        var obj = await _context.UserProfiles
            .Where(x => x.UserId == _user.Id!)
            .Select(x => new { x.PreferredLanguage, x.FirstName })
            .FirstOrDefaultAsync();

        var url = _clientAppSettings.GetEmailConfirmationUrl(token, email, !await _identityService.DoesUserHavePassword(_user.Id!));

        var model = new EmailConfirmationEmailModel
        {
            Language = obj?.PreferredLanguage ?? _localizer.DefaultCulture,
            Subject = _localizer.Get(nameof(SharedResource.EmailConfirmationEmailSubject), CommonStrings.AppName),
            Greeting = _localizer.Get(nameof(SharedResource.EmailGreeting), obj?.FirstName ?? _localizer.Get(nameof(SharedResource.User))),
            Body = _localizer.Get(nameof(SharedResource.EmailConfirmationEmailBody), CommonStrings.AppName, url),
            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
        };
        
        await _emailService.SendEmailAsync(model, email);

        return Result.Success();
    }
}
