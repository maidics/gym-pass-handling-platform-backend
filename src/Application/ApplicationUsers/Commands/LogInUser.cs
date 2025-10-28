using System.Security.Authentication;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using Microsoft.Extensions.Logging;

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
            .NotEmptyWithMaxLenghtAndMessage(nameof(LogInUserCommand.Email), MaxStringLengths.Email)
            .ValidEmailAddress(nameof(LogInUserCommand.Email));

        RuleFor(v => v.Password)
            .NotEmptyWithMaxLenghtAndMessage(nameof(LogInUserCommand.Password), MaxStringLengths.Password)
            .MinimumLengthWithMessage(nameof(LogInUserCommand.Password), MinStringLengths.Password);
    }
}

public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LogInUserCommandHandler> _logger;

    public LogInUserCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService, ILogger<LogInUserCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }
    public async Task<TokenResponse> Handle(LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.AuthenticateUserAsync(command.Email, command.Password, cancellationToken);

        if (result.IsInvalidCredentialsFailure())
        {
            throw new InvalidCredentialException();
        }

        if (!result.Succeeded)
        {
            LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.AuthenticateUserAsync), null, command.Email, result);
            throw new Exception("Failed to authenticate user.");
        }

        var userId = await _identityService.GetUserIdByEmail(command.Email);

        if (userId == null)
        {
            _logger.LogCritical("User authentication succeeded but failed to find user with '{UserEmail}' email after.", command.Email);
            throw new Exception("User authentication succeeded but failed to find user with email after.");
        }

        var jwtResponse = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return jwtResponse;
    }
}
