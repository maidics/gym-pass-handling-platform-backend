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
) : IRequest<Result<Jwt>>;

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

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<Jwt>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    public ResetPasswordCommandHandler(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    public async Task<Result<Jwt>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = Uri.UnescapeDataString(command.EncodedUserId);
        var passwordResetToken = Uri.UnescapeDataString(command.EncodedPasswordResetToken);

        var result = await _identityService.ResetPasswordAsync(userId, passwordResetToken, command.NewPassword);

        if (!result.Succeeded)
        {
            return new ResultFailure(result);
        }

        var jwt = await _jwtService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(jwt);
    }
}
