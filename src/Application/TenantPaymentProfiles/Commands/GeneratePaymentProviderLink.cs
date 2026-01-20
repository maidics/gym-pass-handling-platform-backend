using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.TenantPaymentProfiles.DTOs;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

public record GeneratePaymentProviderLinkCommand(PaymentProviderLinkType Type) : IRequest<Result<PaymentProviderLinkDto>>;

public class GeneratePaymentProviderLinkCommandHandler : IRequestHandler<GeneratePaymentProviderLinkCommand, Result<PaymentProviderLinkDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILocalizer _localizer;
    private readonly IPaymentTenantService _paymentTenantService;

    public GeneratePaymentProviderLinkCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        ILocalizer localizer,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
        _paymentTenantService = paymentTenantService;
    }
    
    public async Task<Result<PaymentProviderLinkDto>> Handle(GeneratePaymentProviderLinkCommand command, CancellationToken cancellationToken)
    {
        var gymId = await _context.GymEmployments
            .AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymId, "Employee gym id", _user.Id);

        var paymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymId);

        if (paymentProfile is null)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
        }

        return command.Type switch
        {
            PaymentProviderLinkType.AccountLink =>
                await _paymentTenantService.GenerateAccountLinkAsync(paymentProfile.PaymentAccountId, gymId,
                    cancellationToken: cancellationToken),

            PaymentProviderLinkType.LoginLink =>
                await _paymentTenantService.GenerateLoginLinkAsync(paymentProfile.PaymentAccountId,
                    cancellationToken: cancellationToken),

            _ => throw new InvalidOperationException(
                $"Invalid/ not implemented {nameof(PaymentProviderLinkType)}: '${command.Type}'")
        };
    }
}
