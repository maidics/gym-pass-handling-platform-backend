using Fitpass.Application.Common.Exceptions;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
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
    ): IRequest<TokenResponse>;

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

public class RegisterPendingGymEmployeeCommandHandler : IRequestHandler<RegisterPendingGymEmployeeCommand, TokenResponse>
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
    public async Task<TokenResponse> Handle(RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.BeginTransactionAsync();

        string userId;

        try
        {
            var result = await _identityService.CreateUserAsync(command.Email, command.Password, command.FirstName, command.LastName);

            if (!result.result.Succeeded)
            {
                await transaction.RollbackAsync();

                _logger.LogError("{Role} registration failed. Result: {Result}", Roles.PendingGymEmployee, result);

                throw new BadRequestException(string.Join(", ", result.result.Errors));
            }

            var roleResult = await _identityService.AddToRoleAsync(result.userId!, Roles.GymAdministrator);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                _logger.LogError("Failed to add ({UserId}) user to {Role}. Result: {Result}", result.userId, Roles.PendingGymEmployee, roleResult);

                throw new BadRequestException(string.Join(", ", roleResult.Errors));
            }

            await transaction.CommitAsync();

            userId = result.userId!;
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught.");

            await transaction.RollbackAsync();

            throw;
        }

        var token = await _jwtTokenService.GenerateTokenAsync(userId, cancellationToken);

        return token;
    }
}
