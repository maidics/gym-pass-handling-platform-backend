using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;

namespace FitPass.Application.Users.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GymEmployeeRegisterUserCommand(string Email, string FirstName, string LastName) : IRequest<Result<GymMembershipDto>>;

public class GymEmployeeRegisterUserCommandValidator : AbstractValidator<GymEmployeeRegisterUserCommand>
{
    public GymEmployeeRegisterUserCommandValidator()
    {
        RuleFor(v => v.Email).ValidEmailAddress(nameof(GymEmployeeRegisterUserCommand.Email));

        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(GymEmployeeRegisterUserCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(GymEmployeeRegisterUserCommand.LastName), MaxStringLengths.Name);
    }
}

public class GymEmployeeRegisterUserCommandHandler : IRequestHandler<GymEmployeeRegisterUserCommand, Result<GymMembershipDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ISender _sender;

    public GymEmployeeRegisterUserCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService,
        ISender sender)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
        _sender = sender;
    }
    public async Task<Result<GymMembershipDto>> Handle(GymEmployeeRegisterUserCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            return Result.Conflict("Email");
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

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.User, true, roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                UserId = creationResultObj.userId!,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            var gymMembership = new GymMembership
            {
                UserId = creationResultObj.userId!,
                GymId = gymEmployment.GymId!
            };

            await _context.GymMemberships.AddAsync(gymMembership);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            //send this with Domain event instead
            //await _sender.Send(new SendEmailConfirmationEmailCommand(command.Email));

            return Result.Success(gymMembership.MapToDto());
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
