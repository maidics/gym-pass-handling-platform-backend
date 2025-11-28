using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record GenerateTenantLoginLinkCommand : IRequest<Result<string>>;

public class GenerateTenantLoginLinkCommandHandler : IRequestHandler<GenerateTenantLoginLinkCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentTenantService _paymentTenantService;

    public GenerateTenantLoginLinkCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _paymentTenantService = paymentTenantService;
    }

    public async Task<Result<string>> Handle(GenerateTenantLoginLinkCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _user.Id, cancellationToken);

        Guard.Against.NullEntityRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var paymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId, cancellationToken);

        if (paymentProfile is null)
        {
            return Result.BusinessRuleViolation("You must first onboard your gym before this action: create your tenant payment account.");
        }

        return await _paymentTenantService.GenerateLoginLinkAsync(paymentProfile.PaymentAccountId, cancellationToken);
    }
}