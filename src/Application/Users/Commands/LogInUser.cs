using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.Users.Commands;

public record LogInUserCommand(
        string Email,
        string Password
    ) : IRequest<Result<JwtToken>>;

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

public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public LogInUserCommandHandler(
        IIdentityService identityService, 
        IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<Result<JwtToken>> Handle(LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.AuthenticateUserAsync(command.Email, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            return new ResultFailure(result);
        }

        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        Guard.Against.Null(userId, "user id", "User was authenticated but then not found after by email.");

        var token = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(token);
    }
}
