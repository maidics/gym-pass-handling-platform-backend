using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Events.Users;
using FitPass.Domain.Strings;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.Users.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GymEmployeeRegisterUserCommand(string Email, string FirstName, string LastName) : IRequest<Result<GymMembershipDto>>;

public class GymEmployeeRegisterUserCommandValidator : AbstractValidator<GymEmployeeRegisterUserCommand>
{
    public GymEmployeeRegisterUserCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Email)
            .EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.FirstName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.FirstName), MaxLengths.Name);

        RuleFor(v => v.LastName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.LastName), MaxLengths.Name);
    }
}

public class GymEmployeeRegisterUserCommandHandler : IRequestHandler<GymEmployeeRegisterUserCommand, Result<GymMembershipDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ILocalizer _localizer;
    private readonly TimeProvider _timeProvider;

    public GymEmployeeRegisterUserCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ILocalizer localizer,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _localizer = localizer;
        _timeProvider = timeProvider;
    }
    public async Task<Result<GymMembershipDto>> Handle(GymEmployeeRegisterUserCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict(
                _localizer.GetWithParamsLocalized(nameof(SharedResource.Conflict), nameof(SharedResource.Email)));
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var creationResultObj = await _identityService.CreateUserAsync(command.Email);

            if (!creationResultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception("Failed to create user.");
            }

            var roleResult = await _identityService.AddToRoleAsync(creationResultObj.userId!, Roles.User);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception($"Failed to add user to role. Result: {roleResult}.");
            }

            var userProfile = new UserProfile
            {
                UserId = creationResultObj.userId!,
                FirstName = command.FirstName,
                LastName = command.LastName,
                PreferredLanguage = _localizer.DefaultCulture,
                CreatedOn = _timeProvider.GetUtcNow()
            };

            await _context.UserProfiles.AddAsync(userProfile);

            userProfile.AddDomainEvent(new UserRegisteredEvent(
                creationResultObj.userId!, 
                command.Email));

            var gymMembership = new GymMembership
            {
                UserId = creationResultObj.userId!,
                GymId = gymEmployment.GymId!
            };

            await _context.GymMemberships.AddAsync(gymMembership);
            
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Result.Success(gymMembership.MapToDto());
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
