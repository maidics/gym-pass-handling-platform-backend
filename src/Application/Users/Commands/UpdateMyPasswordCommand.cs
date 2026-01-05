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
    string NewPasswordConfirm) : IRequest<Result>;

public class UpdateMyPasswordCommandValidator : AbstractValidator<UpdateMyPasswordCommand>
{
    public UpdateMyPasswordCommandValidator(ILocalizer localizer)
    {
        RuleFor(x => x.CurrentPassword).NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.OldPassword));
        
        RuleFor(x => x.NewPassword)
            .StrongPasswordLocalized(localizer);

        RuleFor(x => x.NewPasswordConfirm)
            .Equal(x => x.NewPassword)
            .WithMessage(localizer.Get(nameof(SharedResource.PasswordsMustMatch)));
    }
}

public class UpdateMyPasswordCommandHandler : IRequestHandler<UpdateMyPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public UpdateMyPasswordCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }
    
    public async Task<Result> Handle(UpdateMyPasswordCommand command, CancellationToken cancellationToken)
    {
        return await _identityService.UpdateUserPasswordAsync(_user.Id!, command.CurrentPassword, command.NewPassword);
    }
}
