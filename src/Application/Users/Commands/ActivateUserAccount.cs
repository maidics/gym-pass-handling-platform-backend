using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.DTOs;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Users.Commands;

public record ActivateUserAccountCommand(
    string EncodedEmail,
    string EncodedEmailConfirmationToken,
    bool SetPassword,
    string? Password,
    string? PasswordConfirm) : IRequest<Result<JwtToken>>;

public class ActivateUserAccountCommandValidator : AbstractValidator<ActivateUserAccountCommand>
{
    public ActivateUserAccountCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.EncodedEmail)
            .NotEmpty();

        RuleFor(v => v.EncodedEmailConfirmationToken)
            .NotEmpty();

        When(v => v.SetPassword == true, () =>
        {
            RuleFor(v => v.Password).NotNull(); //no message - malformed request
            RuleFor(v => v.PasswordConfirm).NotNull();
        });

        When(v => v.SetPassword == false, () =>
        {
            RuleFor(v => v.Password).Null(); //no message - malformed request
            RuleFor(v => v.PasswordConfirm).Null();
        });

        When(v => v.Password != null, () => RuleFor(v => v.Password!)
            .StrongPasswordLocalized(localizer));

        RuleFor(v => v.Password) //they both have to be null or StrongPassword
            .Equal(v => v.PasswordConfirm);
    }
}

public class ActivateUserAccountCommandHandler : IRequestHandler<ActivateUserAccountCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILocalizer _localizer;

    public ActivateUserAccountCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _localizer = localizer;
    }
    
    public async Task<Result<JwtToken>> Handle(ActivateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var email = Uri.UnescapeDataString(command.EncodedEmail);

        var userId = await _identityService.GetUserIdByEmailAsync(email);

        if (userId is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        if (!await _identityService.DoesUserHavePassword(userId) && !command.SetPassword)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.UserHasToSetAPassword)));
        }

        var emailConfirmationToken = Uri.UnescapeDataString(command.EncodedEmailConfirmationToken);

        var emailResult = await _identityService.ConfirmEmailAsync(email, emailConfirmationToken);

        if(!emailResult.Succeeded)
        {
            return new ResultFailure(emailResult);
        }

        if (command.SetPassword)
        {
            var passwordResult = await _identityService.AddPasswordToUserWithNoPasswordAsync(email, command.Password!);

            if (!passwordResult.Succeeded)
            {
                return new ResultFailure(passwordResult);
            }
        }

        var token = await _jwtTokenService.GenerateTokenAsync(userId, CancellationToken.None);

        return Result.Success(token);
    }
}
