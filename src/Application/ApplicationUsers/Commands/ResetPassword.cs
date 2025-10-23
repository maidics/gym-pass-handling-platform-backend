using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;

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
        RuleFor(v => v.UserId).NotEmptyWithMessage("User id");

        RuleFor(v => v.PasswordResetToken).NotEmptyWithMessage("Password reset token");

        RuleFor(v => v.NewPassword).StrongPassword();

        RuleFor(v => v.NewPasswordConfirm)
            .NotEmptyWithMessage("New password confirm")
            .Equal(v => v.NewPassword).WithMessage("New password and new password confirm must match.");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(command.UserId);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var passwordResetResult = await _identityService.ResetPasswordAsync(user, command.PasswordResetToken, command.NewPassword);

        if (!passwordResetResult.Succeeded)
        {
            throw new InvalidOperationException("Password reset returned with failure.");
        }
    }
}
