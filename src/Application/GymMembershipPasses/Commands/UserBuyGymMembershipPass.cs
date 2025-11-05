/*
using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Application.GymMembershipPasses.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.GymMembershipPasses.Commands;

[Authorize(Roles = Roles.User)]
public record UserBuyGymMembershipPassCommand(string GymPassProductId) : IRequest<GymMembershipPassDto>;

public class UserBuyGymMembershipPassCommandValidator : AbstractValidator<UserBuyGymMembershipPassCommand>
{
    public UserBuyGymMembershipPassCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage(nameof(UserBuyGymMembershipPassCommand.GymPassProductId));
    }
}

public class UserBuyGymMembershipPassCommandHandler : IRequestHandler<UserBuyGymMembershipPassCommand, GymMembershipPassDto>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserBuyGymMembershipPassCommandHandler> _logger;
    private readonly IMapper _mapper;

    public UserBuyGymMembershipPassCommandHandler(
        IUser user, 
        IApplicationDbContext context, 
        ILogger<UserBuyGymMembershipPassCommandHandler> logger,
        IMapper mapper)
    {
        _user = user;
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<GymMembershipPassDto> Handle(UserBuyGymMembershipPassCommand command, CancellationToken cancellationToken)
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
                    ApplicationUserId = _user.Id!,
                    GymId = gymPassProduct.GymId
                };

                await _context.GymMemberships.AddAsync(userGymMembership);
            }

            /* TODO: handle gym member & gym statuses to prevent user from buying passes in those cases
            if (userGymMembership.Status == Domain.Enums.GymMembershipStatus.Banned)
            {
                throw new ForbiddenAccessException();
            }

            var pass = new GymMembershipPass
            {
                GymMembershipId = userGymMembership.Id,
                Type = gymPassProduct.Type,
                TotalUses = gymPassProduct.TotalUses,
                RemainingUses = gymPassProduct.TotalUses,
                ExpirationDate = gymPassProduct.GetExpirationDate()
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

            return _mapper.Map<GymMembershipPassDto>(pass);
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(UserBuyGymMembershipPassCommandHandler), ex);

            throw;
        }
    }
}
*/
