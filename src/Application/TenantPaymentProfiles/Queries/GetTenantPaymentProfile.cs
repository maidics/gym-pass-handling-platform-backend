using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.TenantPaymentProfiles.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Application.Common.Resources;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.TenantPaymentProfiles.Queries;

[Authorize(Roles = Roles.GymAdministrator)]
public record GetMyTenantPaymentProfileQuery : IRequest<Result<TenantPaymentProfileDto>>;

public class GetMyTenantPaymentProfileQueryHandler : IRequestHandler<GetMyTenantPaymentProfileQuery, Result<TenantPaymentProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;

    public GetMyTenantPaymentProfileQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _localizer = localizer;
    }

    public async Task<Result<TenantPaymentProfileDto>> Handle(GetMyTenantPaymentProfileQuery request, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var dto = await _context
            .TenantPaymentProfiles
            .AsNoTracking()
            .Select(x => x.MapToDto())
            .FirstOrDefaultAsync(tpp => tpp.GymId == gymEmployment.GymId, cancellationToken);

        if (dto is null)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
        }

        return Result.Success(dto);
    }
}
