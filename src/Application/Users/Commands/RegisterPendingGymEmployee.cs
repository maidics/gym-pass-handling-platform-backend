using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;
using FitPass.Application.Common.Resources;

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
    public RegisterPendingGymEmployeeCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.FirstName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FirstName), MaxLength.Name);

        RuleFor(v => v.LastName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.LastName), MaxLength.Name);

        RuleFor(v => v.Email)
            .EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.Password)
            .StrongPasswordLocalized(localizer);

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(localizer.Get(nameof(SharedResource.PasswordsMustMatch)));
    }
}

public class RegisterPendingGymEmployeeCommandHandler : IRequestHandler<RegisterPendingGymEmployeeCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider  _timeProvider;

    public RegisterPendingGymEmployeeCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        ILocalizer localizer,
        TimeProvider  timeProvider)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }
    public async Task<Result<JwtToken>> Handle(RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict(_localizer.GetWithParamsLocalized(nameof(SharedResource.Conflict), nameof(SharedResource.Email)));
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

                throw new Exception($"Failed to add user to role. Result: {roleResult}");
            }

            var profile = new UserProfile
            {
                UserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName,
                PreferredLanguage = _localizer.DefaultCulture,
                CreatedOn = _timeProvider.GetUtcNow()
            };

            await _context.UserProfiles.AddAsync(profile);

            profile.AddDomainEvent(new UserRegisteredEvent(
                userId,
                command.Email,
                command.FirstName,
                false));

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
