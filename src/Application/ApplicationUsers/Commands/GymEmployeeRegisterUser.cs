using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize(Roles = $"{Roles.GymAdministrator},{Roles.GymStaff}")]
public record GymEmployeeRegisterUserCommand(string Email, string FirstName, string LastName) : IRequest<GymMembershipDto>;

public class GymEmployeeRegisterUserCommandValidator : AbstractValidator<GymEmployeeRegisterUserCommand>
{
    public GymEmployeeRegisterUserCommandValidator()
    {
        RuleFor(v => v.Email).ValidEmailAddress(nameof(GymEmployeeRegisterUserCommand.Email));

        RuleFor(v => v.FirstName).NotEmptyWithMaxLenghtAndMessage(nameof(GymEmployeeRegisterUserCommand.FirstName), MaxStringLengths.Name);

        RuleFor(v => v.LastName).NotEmptyWithMaxLenghtAndMessage(nameof(GymEmployeeRegisterUserCommand.LastName), MaxStringLengths.Name);
    }
}

public class GymEmployeeRegisterUserCommandHandler : IRequestHandler<GymEmployeeRegisterUserCommand, GymMembershipDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GymEmployeeRegisterUserCommand> _logger;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public GymEmployeeRegisterUserCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GymEmployeeRegisterUserCommand> logger,
        IIdentityService identityService,
        IMapper mapper)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _identityService = identityService;
        _mapper = mapper;
    }
    public async Task<GymMembershipDto> Handle(GymEmployeeRegisterUserCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId != null && ge.ApplicationUserId == _user.Id);

        if (gymEmployment == null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(GymEmployment));
            throw new Exception(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        if (await _identityService.IsEmailInUseAsync(command.Email))
        {
            throw new ConflictException(ErrorMessages.PropertyIsAlreadyInUse(nameof(GymEmployeeRegisterUserCommand.Email)));
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var creationResultObj = await _identityService.CreateUserAsync(command.Email);

            if (!creationResultObj.result.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.CreateUserAsync),
                    null,
                    _user.Id,
                    creationResultObj.result);

                throw new Exception($"User creation failed: {string.Join(", ", creationResultObj.result.Errors)}");
            }

            var roleResult = await _identityService.AddToRoleAsync(creationResultObj.userId!, Roles.User);

            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.AddToRoleAsync),
                    null,
                    creationResultObj.userId,
                    roleResult);

                throw new Exception(ErrorMessages.FailedToHandleRole(Roles.User, true, roleResult.Errors));
            }

            var userProfile = new UserProfile
            {
                ApplicationUserId = creationResultObj.userId!,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _context.UserProfiles.AddAsync(userProfile);

            var gymMembership = new GymMembership
            {
                ApplicationUserId = creationResultObj.userId,
                GymId = gymEmployment.GymId
            };

            await _context.GymMemberships.AddAsync(gymMembership);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<GymMembershipDto>(gymMembership);
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(GymEmployeeRegisterUserCommand), ex);

            throw;
        }
    }
}