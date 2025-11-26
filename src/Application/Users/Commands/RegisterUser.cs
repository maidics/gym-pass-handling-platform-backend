using FitPass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using FitPass.Application.Common.Models;

namespace FitPass.Application.ApplicationUsers.Commands;

public record RegisterUserCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<Result<JwtToken>>;

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

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<Result<JwtToken>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict("Email");
        }

        string userId;

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var userCreationResultObj = await _identityService.CreateUserAsync(command.Email, command.Password);

            if (!userCreationResultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                return new ResultFailure(userCreationResultObj.result);
            }

            userId = userCreationResultObj.userId!;

            var roleResult = await _identityService.AddToRoleAsync(userCreationResultObj.userId!, Roles.User);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.User, true, roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                UserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            //user.AddDomainEvent(new UserRegisteredEvent(user));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }

        var jwtResponse = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(jwtResponse);
    }
}
