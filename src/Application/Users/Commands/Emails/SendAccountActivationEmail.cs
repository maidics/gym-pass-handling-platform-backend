using FitPass.Application.Common.EmailModels.Users;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Settings;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Options;

namespace FitPass.Application.Users.Commands.Emails;

//for users who have no password
public record SendAccountActivationEmailCommand(string Email, string? UserId = null) : IRequest<Result>;

public class SendAccountActivationEmailCommandValidator : AbstractValidator<SendAccountActivationEmailCommand>
{
    public SendAccountActivationEmailCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Email).EmailAddressWithMessageLocalized(localizer);
    }
}

public class SendAccountActivationEmailCommandHandler : IRequestHandler<SendAccountActivationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ClientAppSettings _clientAppSettings;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILocalizer _localizer;

    public SendAccountActivationEmailCommandHandler(
        IIdentityService identityService,
        IUser user,
        IOptions<ClientAppSettings> options,
        IApplicationDbContext context,
        IEmailService emailService,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _clientAppSettings = options.Value;
        _context = context;
        _emailService = emailService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(SendAccountActivationEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId ?? await _identityService.GetUserIdByEmailAsync(command.Email);

        if (string.IsNullOrEmpty(userId))
        {
            return Result.Success(); //always returning success if the email is valid so we don't leak the email uses
        }
        
        if (await _identityService.IsUserEmailConfirmed(userId))
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.EmailIsAlreadyConfirmed)));
        }

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

        Guard.Against.Null(token, "Email confirmation token", "Failed to generate email confirmation token.");
        
        var obj = await _context.UserProfiles
            .Where(x => x.UserId == userId)
            .Select(x => new { x.PreferredLanguage, x.FirstName })
            .FirstOrDefaultAsync();

        var url = _clientAppSettings.GetAccountActivationUrl(token, command.Email, !await _identityService.DoesUserHavePassword(userId));

        var model = new EmailConfirmationEmailModel
        {
            Language = obj?.PreferredLanguage ?? _localizer.DefaultCulture,
            Subject = _localizer.Get(nameof(SharedResource.EmailConfirmationEmailSubject), CommonStrings.AppName),
            Greeting = _localizer.Get(nameof(SharedResource.EmailGreeting), obj?.FirstName ?? _localizer.Get(nameof(SharedResource.User))),
            Body = _localizer.Get(nameof(SharedResource.EmailConfirmationEmailBody), CommonStrings.AppName, url),
            Farewell = _localizer.Get(nameof(SharedResource.EmailFarewell), CommonStrings.AppName)
        };
        
        await _emailService.SendEmailAsync(model, command.Email);

        return Result.Success();
    }
}
