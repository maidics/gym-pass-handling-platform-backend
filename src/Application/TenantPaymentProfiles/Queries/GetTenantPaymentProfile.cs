using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.TenantPaymentProfiles.Queries;

[Authorize(Roles = Roles.GymAdministrator)]
public record GetTenantPaymentProfileQuery : IRequest<Result<TenantPaymentProfileDto>>;

public class GetTenantPaymentProfileQueryHandler : IRequestHandler<GetTenantPaymentProfileQuery, Result<TenantPaymentProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetTenantPaymentProfileQueryHandler> _logger;

    public GetTenantPaymentProfileQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetTenantPaymentProfileQueryHandler> logger
    )
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    public async Task<Result<TenantPaymentProfileDto>> Handle(GetTenantPaymentProfileQuery request, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger, 
                _user.Roles, 
                _user.Id, 
                nameof(GymEmployment));

            return Result.InternalError(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var tenantPaymentProfile = await _context
            .TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tpp => tpp.GymId == gymEmployment.GymId);

        if (tenantPaymentProfile is null)
        {
            return Result.NotFound(nameof(TenantPaymentProfile));
        }

        return Result.Success(tenantPaymentProfile.MapToDto());
    }
}