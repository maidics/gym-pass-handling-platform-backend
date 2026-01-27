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
    private readonly TimeProvider _timeProvider;

    public GeneratePaymentProviderLinkCommandHandler(
        IApplicationDbContext context, 
        IUser user,
        ILocalizer localizer,
        IPaymentTenantService paymentTenantService,
        TimeProvider timeProvider)
    {
        _context = context;
        _user = user;
        _localizer = localizer;
        _paymentTenantService = paymentTenantService;
        _timeProvider = timeProvider;
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
            .FirstOrDefaultAsync(x => x.GymId == gymId);

        if (paymentProfile is null)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
        }

        Result<PaymentProviderLinkDto>? result;

        if (command.Type == PaymentProviderLinkType.AccountLink)
        {
            var onboardingResult =
                await _paymentTenantService.IsOnboardingCompleteAsync(paymentProfile.PaymentAccountId);

            if (!onboardingResult.Succeeded)
            {
                return onboardingResult.ToFailure<PaymentProviderLinkDto>();
            }

            result = await _paymentTenantService.GenerateAccountLinkAsync(
                paymentProfile.PaymentAccountId, gymId,!onboardingResult.Value, true);
        }
        else
        {
            result = await _paymentTenantService.GenerateLoginLinkAsync(paymentProfile.PaymentAccountId,
                cancellationToken: cancellationToken);
        }

        if (result.Succeeded)
        {
            paymentProfile.LastAccountLinkGeneratedBy = _user.Id;
            paymentProfile.LastAccountLinkGeneratedOn = _timeProvider.GetUtcNow();

            await _context.SaveChangesAsync();
        }

        return result;
    }
}
