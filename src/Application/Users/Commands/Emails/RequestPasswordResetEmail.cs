using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;

namespace FitPass.Application.Users.Commands.Emails;

public record RequestPasswordResetEmailCommand(string Email) : IRequest<Result>; 

public class RequestPasswordResetEmailCommandValidator : AbstractValidator<RequestPasswordResetEmailCommand>
{
    public RequestPasswordResetEmailCommandValidator()
    {
        RuleFor(v => v.Email).ValidEmailAddress(nameof(RequestPasswordResetEmailCommand.Email));
    }
}

public class RequestPasswordResetEmailCommandHandler : IRequestHandler<RequestPasswordResetEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public RequestPasswordResetEmailCommandHandler(IIdentityService identityService, IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(RequestPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        if (userId == null)
        {
            return Result.Success(); //the user will only receive the email if an account exists with an email but we're not letting the frontend know if it exists
        }

        var passwordResetToken = await _identityService.GeneratePasswordResetTokenAsync(userId);

        Guard.Against.Null(passwordResetToken, nameof(passwordResetToken), "Failed to generate password reset token.");

        await _emailService.SendPasswordResetEmailAsync(command.Email, passwordResetToken, userId);

        return Result.Success();
    }
}
