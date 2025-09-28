using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record LogInUserCommand
    (
        string Email,
        string Password
    ) : IRequest<string>;

public class LogInUserCommandValidator : AbstractValidator<LogInUserCommand>
{
    public LogInUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmptyWithMessage("Email")
            .EmailAddress().WithMessage("Email is required");

        RuleFor(v => v.Password)
            .NotEmptyWithMessage("Password");
    }
}

public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, string>
{
    private readonly IIdentityService _identityService;

    public LogInUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<string> Handle(LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.AuthenticateUserAsync(command.Email, command.Password, cancellationToken);

        if (!result.result.Succeeded)
        {
            throw new UnauthorizedAccessException(string.Join(", ", result.result.Errors));
        }

        var jwtToken = await _identityService.GenerateJWTTokenAsync(result.user!, cancellationToken);

        return jwtToken;
    }
}
