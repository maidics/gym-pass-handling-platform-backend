using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Constants;

namespace FitPass.Application.Users.Queries;

//for notifying GymAdmins - Webhook only
public record GetAllGymAdminIdsByTenantPaymentAccountIdQuery(string TenantPaymentAccountId) : IRequest<string[]>;

public class GetAllGymAdminIdsByTenantPaymentAccountIdQueryHandler : IRequestHandler<GetAllGymAdminIdsByTenantPaymentAccountIdQuery, string[]>
{
    private readonly IApplicationDbContext _context;

    public GetAllGymAdminIdsByTenantPaymentAccountIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string[]> Handle(GetAllGymAdminIdsByTenantPaymentAccountIdQuery query, CancellationToken cancellationToken)
    {
        return await _context.GymEmployments
            .AsNoTracking()
            .Where(ge => 
                ge.Gym.PaymentProfile != null && 
                ge.Gym.PaymentProfile.PaymentAccountId == query.TenantPaymentAccountId && 
                ge.Role == Roles.GymAdministrator)
            .Select(ge => ge.UserId)
            .ToArrayAsync();
    }
}