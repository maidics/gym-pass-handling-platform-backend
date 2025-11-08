using FitPass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

public record RegisterPendingGymEmployeeCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ): IRequest<JwtToken>;

public class RegisterPendingGymEmployeeCommandValidator : AbstractValidator<RegisterPendingGymEmployeeCommand>
{
    public RegisterPendingGymEmployeeCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterPendingGymEmployeeCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(RegisterPendingGymEmployeeCommand.LastName), MaxStringLengths.Name);

        RuleFor(v => v.Email).ValidEmailAddress(nameof(RegisterPendingGymEmployeeCommand.Email));

        RuleFor(v => v.Password).StrongPassword();

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(ErrorMessages.PropertyMustEqualToAnotherProperty(nameof(RegisterPendingGymEmployeeCommand.Password), nameof(RegisterPendingGymEmployeeCommand.PasswordConfirm)));
    }
}

public class RegisterPendingGymEmployeeCommandHandler : IRequestHandler<RegisterPendingGymEmployeeCommand, JwtToken>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RegisterPendingGymEmployeeCommand> _logger;

    public RegisterPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        ILogger<RegisterPendingGymEmployeeCommand> logger)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _logger = logger;
    }
    public async Task<JwtToken> Handle(RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            throw new ConflictException(ErrorMessages.PropertyIsAlreadyInUse(nameof(RegisterPendingGymEmployeeCommand.Email)));
        }

        using var transaction = await _context.BeginTransactionAsync();

        string userId;

        try
        {
            var resultObj = await _identityService.CreateUserAsync(command.Email, command.Password, CancellationToken.None);

            if (!resultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(_identityService.CreateUserAsync), [Roles.PendingGymEmployee], resultObj.userId, resultObj.result);

                throw new BadRequestException(string.Join(", ", resultObj.result.Errors));
            }

            userId = resultObj.userId!;

            var roleResult = await _identityService.AddToRoleAsync(resultObj.userId!, Roles.PendingGymEmployee);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(_identityService.AddToRoleAsync), [Roles.PendingGymEmployee], userId, roleResult);

                throw new Exception(string.Join(", ", roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                ApplicationUserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(RegisterPendingGymEmployeeCommandHandler), ex);

            await transaction.RollbackAsync();

            throw;
        }

        var token = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return token;
    }
}
