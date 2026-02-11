using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.Users.Commands;

[Authorize(
    Roles = $"{Roles.User},{Roles.PendingGymEmployee},{Roles.GymStaff},{Roles.GymAdministrator}"
)]
public record DeleteMyAccountCommand : IRequest<Result>;

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public DeleteMyAccountCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context
    )
    {
        _identityService = identityService;
        _user = user;
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteMyAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        var userId = _user.Id;

        Guard.Against.Null(userId);

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _identityService.DeleteUserAsync(userId);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new Exception(
                    $"Failed to delete user account. Message: {result.Message} Errors: ${string.Join(", ", result.Errors)}"
                );
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken: cancellationToken
            );

            if (profile is not null) //should be cascade deleted
            {
                _context.UserProfiles.Remove(profile);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}
