using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Strings;

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
