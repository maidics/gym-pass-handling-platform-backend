using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands;

public record ResetPasswordCommand(
    string EncodedUserId,
    string EncodedPasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest<Result>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.EncodedUserId).NotEmptyLocalized(nameof(ResetPasswordCommand.EncodedUserId));

        RuleFor(v => v.EncodedPasswordResetToken).NotEmptyLocalized(nameof(ResetPasswordCommand.EncodedPasswordResetToken));

        RuleFor(v => v.NewPassword).StrongPasswordLocalized();

        RuleFor(v => v.NewPasswordConfirm)
            .NotEmptyLocalized(nameof(ResetPasswordCommand.NewPasswordConfirm))
            .Equal(v => v.NewPassword)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(ResetPasswordCommand.NewPassword), nameof(ResetPasswordCommand.NewPasswordConfirm)));
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = Uri.UnescapeDataString(command.EncodedUserId);
        var passwordResetToken = Uri.UnescapeDataString(command.EncodedPasswordResetToken);

        var result = await _identityService.ResetPasswordAsync(userId, passwordResetToken, command.NewPassword);

        if (!result.Succeeded)
        {
            return result;
        }

        return Result.Success();
    }
}
