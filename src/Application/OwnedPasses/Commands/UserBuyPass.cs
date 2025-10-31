using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.Passes.Commands;

[Authorize(Roles = Roles.User)]
public record UserBuyPassCommand(string GymPassProductId) : IRequest;

public class UserBuyPassCommandValidator : AbstractValidator<UserBuyPassCommand>
{
    public UserBuyPassCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage(nameof(UserBuyPassCommand.GymPassProductId));
    }
}

public class UserBuyPassCommandHandler : IRequestHandler<UserBuyPassCommand>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserBuyPassCommandHandler> _logger;

    public UserBuyPassCommandHandler(IUser user, IApplicationDbContext context, ILogger<UserBuyPassCommandHandler> logger)
    {
        _user = user;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(UserBuyPassCommand command, CancellationToken cancellationToken)
    {
        var gymPassProduct = await _context
            .GymPassProducts
            .AsNoTracking()
            .Include(gpp => gpp.Gym)
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId);

        Guard.Against.NotFound(command.GymPassProductId, gymPassProduct, "Pass");

        var userGymMembership = await _context
            .GymMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(ge =>
                ge.ApplicationUserId != null &&
                ge.ApplicationUserId == _user.Id &&
                ge.GymId == gymPassProduct.GymId
            );

        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            if (userGymMembership == null)
            {
                userGymMembership = new GymMembership
                {
                    ApplicationUserId = _user.Id,
                    GymId = gymPassProduct.GymId
                };

                await _context.GymMemberships.AddAsync(userGymMembership);
            }

            var pass = new GymMembershipPass
            {
                GymMembershipId = userGymMembership.Id,
                Type = gymPassProduct.Type,
                TotalUses = gymPassProduct.TotalUses,
                RemainingUses = gymPassProduct.TotalUses,
                ExpirationDate = gymPassProduct.GetExpirationDate(),
                HufPrice = gymPassProduct.HufPrice
            };

            await _context.GymMembershipPasses.AddAsync(pass);

            var receipt = new PurchaseReceipt
            {
                UserPaymentProfileId = userGymMembership.Id,
                GymPassProduct = gymPassProduct
            };

            await _context.PurchaseReceipts.AddAsync(receipt);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(UserBuyPassCommandHandler), ex);

            throw;
        }
    }
}
