using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;

namespace FitPass.Application.ApplicationUsers.Commands;

public record ResetPasswordCommand(
    string UserId,
    string PasswordResetToken,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest<TokenResponse>;

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

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public ResetPasswordCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<TokenResponse> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(command.UserId);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var passwordResetResult = await _identityService.ResetPasswordAsync(user, command.PasswordResetToken, command.NewPassword);

        if (!passwordResetResult.Succeeded)
        {
            throw new ValidationException(string.Join(Environment.NewLine, passwordResetResult.Errors));
        }

        var securityStampResult = await _identityService.UpdateSecurityStampAsync(user);

        if (!securityStampResult.Succeeded)
        {
            //What to do? Options:
            //log error and continue
            //throw InvalidOperationException/ Custom Exception
            //return modified response - add property to TokenResponse: string? Warning
            //roll back password change
            //remove security stamps from db - brute force
            //add logic to have an invalid jwt tokens after table and check if the token was made before the set value then reject requests

            //Solution: log error here + add retry mechanism to UpdateSecurityStampAsync
        }

        var tokenResponse = await _jwtTokenService.GenerateTokenAsync(user, CancellationToken.None);

        return tokenResponse;
    }
}