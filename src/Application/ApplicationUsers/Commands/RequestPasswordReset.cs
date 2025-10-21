using FitPass.Application.Common.Interfaces;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record RequestPasswordResetEmailCommand(string Email) : IRequest; //not validated because if it does not exists then it is not sent but frontend does validate if it is an email

public class RequestPasswordResetEmailCommandHandler : IRequestHandler<RequestPasswordResetEmailCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILocalDevEmailService _emailService;

    public RequestPasswordResetEmailCommandHandler(IIdentityService identityService, ILocalDevEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task Handle(RequestPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByEmailAsync(command.Email);

        if (user == null)
        {
            return;
        }

        var passwordResetToken = await _identityService.GeneratePasswordResetTokenAsync(user);

        await _emailService.SendPasswordResetEmailAsync(user.Email!, passwordResetToken, user.Id);
    }
}