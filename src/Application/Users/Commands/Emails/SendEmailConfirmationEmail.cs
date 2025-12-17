using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Common.Settings;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.Users.Commands.Emails;

[Authorize]
public record SendEmailConfirmationEmailCommand : IRequest<Result>;

public class SendEmailConfirmationEmailCommandHandler : IRequestHandler<SendEmailConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly IEmailService _emailService;
    private readonly ILocalizer _localizer;

    public SendEmailConfirmationEmailCommandHandler(
        IIdentityService identityService,
        IUser user,
        ClientAppSettings  clientAppSettings,
        IEmailService emailService,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _user = user;
        _clientAppSettings = clientAppSettings;
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

        var url = _clientAppSettings.GetEmailConfirmationUrl(token, email, !await _identityService.DoesUserHavePassword(_user.Id!));

        //TODO: await _emailService.SendEmailAsync(email, EmailSubjects.EmailConfirmation(), EmailBodies.EmailConfirmation(url));

        return Result.Success();
    }
}
