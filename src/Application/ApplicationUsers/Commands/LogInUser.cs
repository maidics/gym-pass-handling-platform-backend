using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record LogInUserCommand
    (
        string Email,
        string Password
    ) : IRequest<TokenResponse>;

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

public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public LogInUserCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<TokenResponse> Handle(LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.AuthenticateUserAsync(command.Email, command.Password, cancellationToken);

        if (!result.result.Succeeded)
        {
            throw new UnauthorizedAccessException(string.Join(", ", result.result.Errors));
        }

        var jwtResponse = await _jwtTokenService.GenerateTokenAsync(result.user!, cancellationToken);

        return jwtResponse;
    }
}
