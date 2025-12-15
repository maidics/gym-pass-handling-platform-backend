using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.Users.Commands;

public record ResetPasswordCommand(
    string EncodedUserId,
    string EncodedPasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest<Result>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.EncodedUserId).NotEmpty();

        RuleFor(v => v.EncodedPasswordResetToken).NotEmpty();

        RuleFor(v => v.NewPassword).NotEmpty();

        RuleFor(v => v.NewPasswordConfirm)
            .NotEmpty()
            .WithMessage(localizer.Get(
                nameof(SharedResource.PropertyIsRequired), localizer.GetWithParamsLocalized(nameof(SharedResource.NewValue), nameof(SharedResource.Password))))
            .Equal(v => v.NewPassword)
            .WithMessage(localizer.Get(nameof(SharedResource.PasswordsMustMatch)));
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
