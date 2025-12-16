using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Models;
using FitPass.Application.Users.DTOs;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.Users.Commands;

public record RegisterUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PasswordConfirm
    ) : IRequest<Result<JwtToken>>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.FirstName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FirstName), MaxLength.Name);

        RuleFor(v => v.LastName!)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.LastName), MaxLength.Name);

        RuleFor(v => v.Email).EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.Password).StrongPasswordLocalized(localizer);

        RuleFor(v => v.PasswordConfirm)
            .Equal(v => v.PasswordConfirm)
            .WithMessage(localizer.Get(nameof(SharedResource.PasswordsMustMatch)));
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<JwtToken>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        ILocalizer localizer,
        TimeProvider  timeProvider)
    {
        _identityService = identityService;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }
    public async Task<Result<JwtToken>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict(_localizer.GetWithParamsLocalized(nameof(SharedResource.Conflict), nameof(SharedResource.Email)));
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

                throw new Exception($"Failed to add user to role. Result: {roleResult}.");
            }

            var userProfile = new UserProfile
            {
                UserId = userId,
                FirstName = command.FirstName,
                LastName = command.LastName,
                PreferredLanguage = _localizer.DefaultCulture,
                CreatedOn = _timeProvider.GetUtcNow()
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
