using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;

namespace FitPass.Application.Users.Commands.Emails;

[Authorize]
//for users that are logged in - already have password
public record SendEmailConfirmationEmailCommand : IRequest<Result>;

public class SendEmailConfirmationEmailCommandHandler
    : IRequestHandler<SendEmailConfirmationEmailCommand, Result>
{
    private readonly ISender _sender;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILocalizer _localizer;

    public SendEmailConfirmationEmailCommandHandler(
        ISender sender,
        IUser user,
        IIdentityService identityService,
        ILocalizer localizer
    )
    {
        _sender = sender;
        _user = user;
        _identityService = identityService;
        _localizer = localizer;
    }

    public async Task<Result> Handle(
        SendEmailConfirmationEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        var email = await _identityService.GetEmailByIdAsync(_user.Id!);

        Guard.Against.NullParameterRelatedToCurrentUser(email, "email", _user.Id);

        if (await _identityService.IsUserEmailConfirmed(_user.Id!))
        {
            return Result.BusinessRuleViolation(
                _localizer.Get(nameof(SharedResource.EmailIsAlreadyConfirmed))
            );
        }

        var command = new SendAccountActivationEmailCommand(email, _user.Id);

        return await _sender.Send(command, cancellationToken);
    }
}
