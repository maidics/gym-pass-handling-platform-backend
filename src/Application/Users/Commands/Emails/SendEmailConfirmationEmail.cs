using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPass.Application.ApplicationUsers.Commands.Emails;

[Authorize]
public record SendEmailConfirmationEmailCommand(string Email) : IRequest<Result>;

public class SendEmailConfirmationEmailCommandValidator : AbstractValidator<SendEmailConfirmationEmailCommand>
{
    public SendEmailConfirmationEmailCommandValidator()
    {
        RuleFor(v => v.Email).ValidEmailAddress(nameof(SendEmailConfirmationEmailCommand.Email));
    }
}

public class SendEmailConfirmationEmailCommandHandler : IRequestHandler<SendEmailConfirmationEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ILogger<SendEmailConfirmationEmailCommandHandler> _logger;
    private readonly IEmailService _emailService;

    public SendEmailConfirmationEmailCommandHandler(
        IIdentityService identityService,
        IUser user,
        IEmailService emailService,
        ILogger<SendEmailConfirmationEmailCommandHandler> logger)
    {
        _identityService = identityService;
        _user = user;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        Guard.Against.NullEntityRelatedToCurrentUser(userId, "User", null);

        if (_user.Id! != userId && _user.Roles!.First() != Roles.GymAdministrator && _user.Roles!.First() != Roles.GymStaff)
        {
            return Result.Forbidden();
        }

        if (await _identityService.IsUserEmailConfirmed(userId))
        {
            return Result.BusinessRuleViolation("User email is already confirmed.");
        }

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

        Guard.Against.Null(token, "Email confirmation token", "Failed to generate email confirmation token.");

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(command.Email);

        //TODO: set url somehow
        var setPassword = !await _identityService.DoesUserHavePassword(userId);

        var url = $"localhost/email-confirmation?token={encodedToken}&user={encodedEmail}&=flag{(setPassword ? 1 : 0)}";

        await _emailService.SendEmailAsync(command.Email, EmailSubjects.EmailConfirmation(), EmailBodies.EmailConfirmation(url));

        return Result.Success();
    }
}
