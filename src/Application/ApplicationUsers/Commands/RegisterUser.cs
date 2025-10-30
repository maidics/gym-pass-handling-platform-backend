using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace Fitpass.Application.ApplicationUsers.Commands;

public record RegisterUserCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<TokenResponse>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterUserCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName!).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterUserCommand.LastName), MaxStringLengths.Name);

        RuleFor(v => v.Email).ValidEmailAddress(nameof(RegisterUserCommand.Email));

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterUserCommand.Password), nameof(RegisterUserCommand.PasswordConfirm)));
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IStripeCustomerService _stripeCustomerService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterUserCommand> _logger;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IStripeCustomerService stripeCustomerService,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterUserCommand> logger)
    {
        _identityService = identityService;
        _context = context;
        _stripeCustomerService = stripeCustomerService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }
    public async Task<TokenResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            throw new ConflictException(ErrorMessages.PropertyIsAlreadyInUse(nameof(RegisterUserCommand.Email)));
        }

        string userId;

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var userCreationResultObj = await _identityService.CreateUserAsync(command.Email, command.Password, command.FirstName, command.LastName);

            if (!userCreationResultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.CreateUserAsync), Roles.User, command.Email, userCreationResultObj.result);

                throw new BadRequestException(string.Join(", ", userCreationResultObj.result.Errors));
            }

            userId = userCreationResultObj.userId!;

            var roleResult = await _identityService.AddToRoleAsync(userCreationResultObj.userId!, Roles.User);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.AddToRoleAsync), Roles.User, userId, roleResult);

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.User, true, roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                ApplicationUserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            var stripeCustomerId = await _stripeCustomerService.CreateStripeCustomer(userProfile, command.Email);

            var paymentProfile = new UserPaymentProfile
            {
                ApplicationUserId = userId,
                StripeCustomerId = stripeCustomerId
            };

            await _context.UserPaymentProfiles.AddAsync(paymentProfile);

            //user.AddDomainEvent(new UserRegisteredEvent(user));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (ex.IsStripeServiceException())
            {
                throw;
            }

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(RegisterUserCommandHandler), ex);

            throw;
        }

        var jwtResponse = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return jwtResponse;
    }
}
