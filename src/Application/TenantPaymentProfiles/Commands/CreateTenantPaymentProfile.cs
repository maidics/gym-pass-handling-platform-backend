using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Application.Common.Resources;
using FitPass.Application.TenantPaymentProfiles.DTOs;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateTenantPaymentProfileCommand(
    string PaymentAccountHolderEmail,
    string BusinessName
) : IRequest<Result<PaymentProviderLinkDto>>;

public class CreateTenantPaymentProfileCommandValidator : AbstractValidator<CreateTenantPaymentProfileCommand>
{
    public CreateTenantPaymentProfileCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.PaymentAccountHolderEmail)
            .EmailAddressWithMessageLocalized(localizer);

        RuleFor(v => v.BusinessName)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.BusinessName), MaxLengths.Name);
    }
}

public class CreateTenantPaymentProfileCommandHandler : IRequestHandler<CreateTenantPaymentProfileCommand, Result<PaymentProviderLinkDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentTenantService _paymentTenantService;
    private readonly ILocalizer _localizer;

    public CreateTenantPaymentProfileCommandHandler(
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

    public async Task<Result<PaymentProviderLinkDto>> Handle(CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var paymentProfile = await _context
            .TenantPaymentProfiles
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId);

        if (paymentProfile is not null)
        {
            return Result.Forbidden(
                _localizer.GetWithParamsLocalized(nameof(SharedResource.ResourceAlreadyExists), nameof(SharedResource.TenantPaymentProfile)));
        }

        var result = await _paymentTenantService
            .CreateTenantAccount(gymEmployment.GymId, command.PaymentAccountHolderEmail, command.BusinessName);

        if (!result.Succeeded)
        {
            return result.ToFailure<PaymentProviderLinkDto>();
        }

        paymentProfile = new TenantPaymentProfile
        {
            GymId = gymEmployment.GymId,
            PaymentAccountId = result.Value
        };

        await _context.TenantPaymentProfiles.AddAsync(paymentProfile);
        await _context.SaveChangesAsync();

        return await _paymentTenantService.GenerateAccountLinkAsync(result.Value, gymEmployment.GymId, true);
    }
}
