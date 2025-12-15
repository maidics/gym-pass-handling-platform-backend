using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record GenerateTenantLoginLinkCommand : IRequest<Result<string>>;

public class GenerateTenantLoginLinkCommandHandler : IRequestHandler<GenerateTenantLoginLinkCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentTenantService _paymentTenantService;
    private readonly ILocalizer _localizer;

    public GenerateTenantLoginLinkCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentTenantService paymentTenantService,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _paymentTenantService = paymentTenantService;
        _localizer = localizer;
    }

    public async Task<Result<string>> Handle(GenerateTenantLoginLinkCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var paymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId, cancellationToken);

        if (paymentProfile is null)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
        }

        return await _paymentTenantService.GenerateLoginLinkAsync(paymentProfile.PaymentAccountId, cancellationToken);
    }
}
