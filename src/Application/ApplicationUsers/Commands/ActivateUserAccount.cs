using FitPass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

public record ActivateUserAccountCommand(
    string EncodedEmail,
    string EncodedEmailConfirmationToken,
    string? Password,
    string? PasswordConfirm) : IRequest<JwtToken>;

public class ActivateUserAccountCommandValidator : AbstractValidator<ActivateUserAccountCommand>
{
    public ActivateUserAccountCommandValidator()
    {
        RuleFor(v => v.EncodedEmail).NotEmptyWithMessage(nameof(ActivateUserAccountCommand.EncodedEmail));

        RuleFor(v => v.EncodedEmailConfirmationToken).NotEmptyWithMessage(nameof(ActivateUserAccountCommand.EncodedEmailConfirmationToken));

        When(v => v.Password != null, () => RuleFor(v => v.Password!).StrongPassword());

        RuleFor(v => v.Password) //they both have to be null or StrongPassword
            .Equal(v => v.PasswordConfirm)
            .WithMessage(
                ErrorMessages.PropertyMustEqualToAnotherProperty(
                    nameof(ActivateUserAccountCommand.Password),
                    nameof(ActivateUserAccountCommand.PasswordConfirm)
                )
            );
    }
}

public class ActivateUserAccountCommandHandler : IRequestHandler<ActivateUserAccountCommand, JwtToken>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ActivateUserAccountCommandHandler> _logger;
    private readonly IJwtTokenService _jwtTokenService;

    public ActivateUserAccountCommandHandler(
        IIdentityService identityService,
        ILogger<ActivateUserAccountCommandHandler> logger,
        IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }
    
    public async Task<JwtToken> Handle(ActivateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var email = Uri.UnescapeDataString(command.EncodedEmail);

        var userId = await _identityService.GetUserIdByEmailAsync(email);

        Guard.Against.NotFound(email, userId, "User");

        var emailConfirmationToken = Uri.UnescapeDataString(command.EncodedEmailConfirmationToken);

        var emailResult = await _identityService.ConfirmEmailAsync(email, emailConfirmationToken);

        if (emailResult.IsResultFailureWithOneErrorMessage(ErrorMessages.TokenIsInvalid("Email confirmation")))
        {
            throw new BadRequestException(string.Join(", ", emailResult.Errors));
        }

        if (!emailResult.Succeeded)
        {
            LogErrorMessages.IdentityServiceMethodFailed(
                _logger,
                nameof(IIdentityService.ConfirmEmailAsync),
                null,
                email,
                emailResult
            );

            throw new Exception(ErrorMessages.FailedToActiveAccount());
        }

        if (command.Password != null && command.PasswordConfirm != null)
        {
            var passwordResult = await _identityService.AddPasswordToUserWithNoPasswordAsync(email, command.Password);

            if (passwordResult.IsResultFailureWithOneErrorMessage(ErrorMessages.UserNotFound()))
            {
                throw new NotFoundException(email, "User");
            }

            if (passwordResult.IsResultFailureWithOneErrorMessage(ResultErrorMessages.UserAlreadyHasPassword()))
            {
                throw new BadRequestException(ResultErrorMessages.UserAlreadyHasPassword());
            }

            if (!passwordResult.Succeeded)
            {
                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.AddPasswordToUserWithNoPasswordAsync),
                    null,
                    email,
                    passwordResult
                );

                throw new Exception("Failed to set password for user.");
            }
        }

        var token = await _jwtTokenService.GenerateTokenAsync(userId, CancellationToken.None);

        return token;
    }
}
