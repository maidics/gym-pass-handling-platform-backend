using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Users.DTOs;

namespace FitPass.Application.Users.Commands;

public record ResetPasswordCommand(
    string EncodedUserId,
    string EncodedPasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest<Result<JwtToken>>;

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

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public ResetPasswordCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<JwtToken>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = Uri.UnescapeDataString(command.EncodedUserId);
        var passwordResetToken = Uri.UnescapeDataString(command.EncodedPasswordResetToken);

        var result = await _identityService.ResetPasswordAsync(userId, passwordResetToken, command.NewPassword);

        if (!result.Succeeded)
        {
            return new ResultFailure(result);
        }

        var jwt = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(jwt);
    }
}
