using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Gyms.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RegisterGymCommand(CreateGymDto CreateGymDto, string PendingGymEmployeeToPromoteEmail) : IRequest<GymDto>;

public class RegisterGymCommandValidator : AbstractValidator<RegisterGymCommand>
{
    public RegisterGymCommandValidator()
    {
        RuleFor(v => v.CreateGymDto).NotEmptyWithMessage(nameof(RegisterGymCommand.CreateGymDto));

        RuleFor(v => v.CreateGymDto.GymName)
            .NotEmptyWithMaxLenghtAndMessage(nameof(RegisterGymCommand.CreateGymDto.GymName), MaxStringLengths.Description);

        RuleFor(v => v.CreateGymDto.GymAddress)
            .NotEmptyWithMaxLenghtAndMessage(nameof(RegisterGymCommand.CreateGymDto.GymAddress), MaxStringLengths.Address);

        RuleFor(v => v.CreateGymDto.EscalationEmail)
            .ValidEmailAddress(nameof(RegisterGymCommand.CreateGymDto.EscalationEmail));

        RuleFor(v => v.PendingGymEmployeeToPromoteEmail)
            .ValidEmailAddress(nameof(RegisterGymCommand.PendingGymEmployeeToPromoteEmail));
    }
}

public class RegisterGymCommandHandler : IRequestHandler<RegisterGymCommand, GymDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly ILogger<RegisterGymCommandHandler> _logger;
    private readonly IMapper _mapper;

    public RegisterGymCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user,
        ILogger<RegisterGymCommandHandler> logger,
        IMapper mapper
    )
    {
        _context = context;
        _identityService = identityService;
        _user = user;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<GymDto> Handle(RegisterGymCommand command, CancellationToken cancellationToken)
    {
        var existingGym = await _context
            .Gyms
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == command.CreateGymDto.GymName);

        if (existingGym is not null)
        {
            throw new ConflictException(ErrorMessages.PropertyIsAlreadyInUse($"Gym name: {command.CreateGymDto.GymName}"));
        }

        var userToPromoteId = await _identityService.GetUserIdByEmailAsync(command.PendingGymEmployeeToPromoteEmail);

        Guard.Against.NotFound(command.PendingGymEmployeeToPromoteEmail, userToPromoteId, "User");

        if (!await _identityService.IsInRoleAsync(userToPromoteId, Roles.PendingGymEmployee))
        {
            throw new BadRequestException("Specified user is not a Pending Gym Employee user.");
        }

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(userToPromoteId, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.RemoveFromRoleAsync),
                    _user.Roles,
                    _user.Id,
                    demotionResult);

                throw new SystemException(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, false, demotionResult.Errors));
            }

            var promotionResult = await _identityService.AddToRoleAsync(userToPromoteId, Roles.GymAdministrator);

            if (!promotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(
                    _logger,
                    nameof(IIdentityService.AddToRoleAsync),
                    _user.Roles,
                    _user.Id,
                    promotionResult);

                throw new SystemException(ErrorMessages.FailedToHandleRole(Roles.PendingGymEmployee, true, promotionResult.Errors));
            }

            var gym = new Gym
            {
                Name = command.CreateGymDto.GymName,
                Address = command.CreateGymDto.GymAddress,
                Status = command.CreateGymDto.GymStatus,
                Tier = command.CreateGymDto.GymTier,
                OwnerName = command.CreateGymDto.GymOwnerName
            };

            await _context.Gyms.AddAsync(gym);


            var gymEmployment = new GymEmployment
            {
                ApplicationUserId = userToPromoteId,
                GymId = gym.Id,
                Role = Roles.GymAdministrator
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<GymDto>(gym);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            LogErrorMessages.UnhandledExceptionCaught(
                _logger,
                nameof(RegisterGymCommandHandler),
                ex);

            throw;
        }
    }
}
