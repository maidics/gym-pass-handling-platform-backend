using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Strings;

namespace Fitpass.Application.ApplicationUsers.Commands.Emails;

public record RequestPasswordResetEmailCommand(string Email) : IRequest; 

public class RequestPasswordResetEmailCommandValidator : AbstractValidator<RequestPasswordResetEmailCommand>
{
    public RequestPasswordResetEmailCommandValidator()
    {
        RuleFor(v => v.Email).ValidEmailAddress(nameof(RequestPasswordResetEmailCommand.Email));
    }
}

public class RequestPasswordResetEmailCommandHandler : IRequestHandler<RequestPasswordResetEmailCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public RequestPasswordResetEmailCommandHandler(IIdentityService identityService, IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task Handle(RequestPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        if (userId == null)
        {
            return;
        }

        var passwordResetToken = await _identityService.GeneratePasswordResetTokenAsync(userId);

        if (passwordResetToken == null)
        {
            throw new Exception(ErrorMessages.FailedtoGeneratePasswordResetToken());
        }

        await _emailService.SendPasswordResetEmailAsync(command.Email, passwordResetToken, userId);
    }
}
