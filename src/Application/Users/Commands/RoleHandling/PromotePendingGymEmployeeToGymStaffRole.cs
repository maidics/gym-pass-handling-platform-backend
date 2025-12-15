using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.Users.Commands.RoleHandling;

[Authorize(Roles = Roles.GymAdministrator)]
public record PromotePendingGymEmployeeToGymStaffRoleCommand (string UserId) : IRequest<Result>;

public class PromotePendingGymEmployeeToGymStaffRoleCommandValidator : AbstractValidator<PromotePendingGymEmployeeToGymStaffRoleCommand>
{
    public PromotePendingGymEmployeeToGymStaffRoleCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.UserId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.User));
    }
}

public class PromotePendingGymEmployeeToGymStaffRoleCommandHandler : IRequestHandler<PromotePendingGymEmployeeToGymStaffRoleCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizer _localizer;

    public PromotePendingGymEmployeeToGymStaffRoleCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ILocalizer localizer)
    {
        _identityService = identityService;
        _user = user;
        _context = context;
        _timeProvider = timeProvider;
        _localizer = localizer;
    }
    public async Task<Result> Handle(PromotePendingGymEmployeeToGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        var promoterGymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(promoterGymEmployment, "Promoter GymEmployment", _user.Id);

        if (!await _identityService.DoesUserExist(command.UserId))
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.User)));
        }

        if (!await _identityService.IsInRoleAsync(command.UserId, Roles.PendingGymEmployee))
        {
            return Result.Forbidden(_localizer.Get(nameof(SharedResource.Forbidden)));
        }

        using var transaction = await _context.BeginTransactionAsync();
        
        try
        {
            var demotionResult = await _identityService.RemoveFromRoleAsync(command.UserId, Roles.PendingGymEmployee);

            if (!demotionResult.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception($"Failed to remove user from their role. Result: {demotionResult}.");
            }

            var promotionResult = await _identityService.AddToRoleAsync(command.UserId, Roles.GymStaff);

            if (!promotionResult.Succeeded) //cannot be user not found if demotion succeeds
            {
                await transaction.RollbackAsync();

                throw new Exception($"Failed to add user to role. Result: {promotionResult}.");
            }

            var gymEmployment = new GymEmployment
            {
                UserId = command.UserId,
                GymId = promoterGymEmployment.GymId,
                Role = Roles.GymStaff,
                EmploymentStart = _timeProvider.GetUtcNow()
            };

            await _context.GymEmployments.AddAsync(gymEmployment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        } catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
}
