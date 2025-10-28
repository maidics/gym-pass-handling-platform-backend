using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Extensions;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

public record ResetPasswordCommand(
    string UserId,
    string PasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(ResetPasswordCommand.UserId));

        RuleFor(v => v.PasswordResetToken).NotEmptyWithMessage(nameof(ResetPasswordCommand.PasswordResetToken));

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
        var result = await _identityService.ResetPasswordAsync(command.UserId, command.PasswordResetToken, command.NewPassword);

        if (!result.Succeeded)
        {
            LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.ResetPasswordAsync), null, command.UserId, result);
            throw new InvalidOperationException("Password reset returned with failure.");
        }
    }
}
