using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace FitPass.Application.Users.Commands;

[Authorize(Roles = $"{Roles.User}, {Roles.PendingGymEmployee}, {Roles.GymStaff}, {Roles.GymAdministrator}")]
public record DeleteMyAccountCommand : IRequest<Result>;

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;

    public DeleteMyAccountCommandHandler(
        IIdentityService identityService,
        IUser user,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _user = user;
        _context = context;
    }

    public async Task<Result> Handle(DeleteMyAccountCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var result = await _identityService.DeleteUserAsync(_user.Id!);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                throw new Exception($"Failed to delete user account.");
            }

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
