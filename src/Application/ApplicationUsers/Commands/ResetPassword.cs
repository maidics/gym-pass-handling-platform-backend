using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

public record ResetPasswordCommand(
    string EncodedUserId,
    string EncodedPasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.EncodedUserId).NotEmptyWithMessage(nameof(ResetPasswordCommand.EncodedUserId));

        RuleFor(v => v.EncodedPasswordResetToken).NotEmptyWithMessage(nameof(ResetPasswordCommand.EncodedPasswordResetToken));

        RuleFor(v => v.NewPassword).StrongPassword();

        RuleFor(v => v.NewPasswordConfirm)
            .NotEmptyWithMessage(nameof(ResetPasswordCommand.NewPasswordConfirm))
            .Equal(v => v.NewPassword)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(ResetPasswordCommand.NewPassword), nameof(ResetPasswordCommand.NewPasswordConfirm)));
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(IIdentityService identityService, ILogger<ResetPasswordCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = Uri.UnescapeDataString(command.EncodedUserId);
        var passwordResetToken = Uri.UnescapeDataString(command.EncodedPasswordResetToken);

        var result = await _identityService.ResetPasswordAsync(userId, passwordResetToken, command.NewPassword);

        if (result.IsResultFailureWithOneErrorMessage(ErrorMessages.UserNotFound()))
        {
            throw new NotFoundException(userId, "User");
        }

        if (!result.Succeeded)
        {
            LogErrorMessages.IdentityServiceMethodFailed(
                _logger,
                nameof(IIdentityService.ResetPasswordAsync),
                null,
                userId,
                result);
                
            throw new InvalidOperationException("Password reset returned with failure.");
        }
    }
}
