using System.Text.Encodings.Web;
using FitPass.Application.Common.Configuration;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPass.Application.ApplicationUsers.Commands.Emails;

[Authorize]
public record SendEmailConfirmationEmailCommand(string Email) : IRequest<Result>;
//instead of throwing exceptions it will return a result => user can always request a new email confirmation email
//in case of just solely sending email confirmation email the minimal api method will check the result and return a http result accordingly

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
    private readonly ILogger<SendEmailConfirmationEmailCommandHandler> _logger;
    private readonly ILocalDevEmailService _emailService;
    private readonly FrontendSettings _frontendSettings;

    public SendEmailConfirmationEmailCommandHandler(
        IIdentityService identityService,
        ILocalDevEmailService emailService,
        ILogger<SendEmailConfirmationEmailCommandHandler> logger,
        IOptions<FrontendSettings> options)
    {
        _identityService = identityService;
        _emailService = emailService;
        _logger = logger;
        _frontendSettings = options.Value;
    }

    public async Task<Result> Handle(SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        Guard.Against.NotFound(command.Email, userId, "User");

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

        if (token == null)
        {
            LogErrorMessages.IdentityServiceMethodFailed(
                _logger,
                nameof(IIdentityService.GenerateEmailConfirmationTokenAsync),
                null,
                userId,
                null);

            return Result.Failure([ErrorMessages.FailedtoGenerateEmailConfirmationToken()]);
        }

        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(command.Email);

        var url = $"{_frontendSettings.BaseUrl}{_frontendSettings.EmailConfirmationPath}?token={encodedToken}&user={encodedEmail}";

        await _emailService.SendEmailAsync(command.Email, EmailSubjects.EmailConfirmation(), EmailBodies.EmailConfirmation(url));

        return Result.Success();
    }
}
