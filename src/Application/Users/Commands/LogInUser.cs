using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Constants;

namespace FitPass.Application.Users.Commands;

public record LogInUserCommand(string Email, string Password) : IRequest<Result<Jwt>>;

public class LogInUserCommandValidator : AbstractValidator<LogInUserCommand>
{
    public LogInUserCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Email).EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.Password)
            .NotEmptyWithMinLengthAndMessageLocalized(
                localizer,
                nameof(SharedResource.Password),
                MinLengths.Password
            );
    }
}

public class LogInUserCommandHandler : IRequestHandler<LogInUserCommand, Result<Jwt>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    public LogInUserCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService
    )
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    public async Task<Result<Jwt>> Handle(
        LogInUserCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _identityService.AuthenticateUserAsync(command.Email, command.Password);

        if (!result.Succeeded)
        {
            return new ResultFailure(result);
        }

        var userId = await _identityService.GetUserIdByEmailAsync(command.Email);

        Guard.Against.Null(
            userId,
            "user id",
            "User was authenticated but then not found after by email."
        );

        var token = await _jwtService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(token);
    }
}
