using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands;

public record RegisterPendingGymEmployeeCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ): IRequest<Result<JwtToken>>;

public class RegisterPendingGymEmployeeCommandValidator : AbstractValidator<RegisterPendingGymEmployeeCommand>
{
    public RegisterPendingGymEmployeeCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessageLocalized(nameof(RegisterPendingGymEmployeeCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessageLocalized(nameof(RegisterPendingGymEmployeeCommand.LastName), MaxStringLengths.Name);

        RuleFor(v => v.Email).ValidEmailAddressWithMessageLocalized(nameof(RegisterPendingGymEmployeeCommand.Email));

        RuleFor(v => v.Password).StrongPasswordLocalized();

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterPendingGymEmployeeCommand.Password), nameof(RegisterPendingGymEmployeeCommand.PasswordConfirm)));
    }
}

public class RegisterPendingGymEmployeeCommandHandler : IRequestHandler<RegisterPendingGymEmployeeCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public RegisterPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }
    public async Task<Result<JwtToken>> Handle(RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict("Email");
        }

        using var transaction = await _context.BeginTransactionAsync();

        string userId;

        try
        {
            var resultObj = await _identityService.CreateUserAsync(command.Email, command.Password, CancellationToken.None);

            if (!resultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                return new ResultFailure(resultObj.result);
            }

            userId = resultObj.userId!;

            var roleResult = await _identityService.AddToRoleAsync(resultObj.userId!, Roles.PendingGymEmployee);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, true, roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                UserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }

        var token = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return Result.Success(token);
    }
}
