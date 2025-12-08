using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands.Emails;

[Authorize]
public record SendEmailConfirmationEmailCommand : IRequest<Result>;

public class SendEmailConfirmationEmailCommandHandler : IRequestHandler<SendEmailConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IEmailService _emailService;

    public SendEmailConfirmationEmailCommandHandler(
        IIdentityService identityService,
        IUser user,
        IEmailService emailService)
    {
        _identityService = identityService;
        _user = user;
        _emailService = emailService;
    }

    public async Task<Result> Handle(SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation("Email is already confirmed.");
        }

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(_user.Id!);

        Guard.Against.Null(token, "Email confirmation token", "Failed to generate email confirmation token.");

        var email = await _identityService.GetEmailByIdAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(email, "Email", _user.Id);

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);

        //TODO: set url somehow
        var setPassword = !await _identityService.DoesUserHavePassword(_user.Id!);

        var url = $"localhost/email-confirmation?token={encodedToken}&user={encodedEmail}&=flag{(setPassword ? 1 : 0)}";

        await _emailService.SendEmailAsync(email, EmailSubjects.EmailConfirmation(), EmailBodies.EmailConfirmation(url));

        return Result.Success();
    }
}
