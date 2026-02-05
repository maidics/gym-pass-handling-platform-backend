using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;

namespace FitPass.Application.Users.Commands;

[Authorize]
public record UpdateMyPasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string NewPasswordConfirm
) : IRequest<Result>;

public class UpdateMyPasswordCommandValidator : AbstractValidator<UpdateMyPasswordCommand>
{
    public UpdateMyPasswordCommandValidator(ILocalizer localizer)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.OldPassword));

        RuleFor(x => x.NewPassword).StrongPasswordLocalized(localizer);

        RuleFor(x => x.CurrentPassword)
            .NotEqual(x => x.NewPassword)
            .WithMessage(localizer.Get(nameof(SharedResource.NewPasswordCannotBeSameAsOld)));

        RuleFor(x => x.NewPasswordConfirm)
            .Equal(x => x.NewPassword)
            .WithMessage(localizer.Get(nameof(SharedResource.PasswordsMustMatch)));
    }
}

public class UpdateMyPasswordCommandHandler : IRequestHandler<UpdateMyPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public UpdateMyPasswordCommandHandler(
        IIdentityService identityService,
        IUser user,
        ILocalizer localizer
    )
    {
        _identityService = identityService;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        UpdateMyPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = await _identityService.GetEmailByIdAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(email, nameof(email), _user.Id);

        //no need to check if new password is the same as the previous because if the passed current password is not correct this will return &
        //the validator throws if the current & new passwords are the same
        var result = await _identityService.AuthenticateUserAsync(email, command.CurrentPassword);

        if (!result.Succeeded)
        {
            return Result.Unauthorized(
                _localizer.Get(
                    nameof(SharedResource.ValueIsInvalid),
                    _localizer.Get(nameof(SharedResource.Password))
                )
            );
        }

        return await _identityService.UpdateUserPasswordAsync(
            _user.Id!,
            command.CurrentPassword,
            command.NewPassword
        );
    }
}
