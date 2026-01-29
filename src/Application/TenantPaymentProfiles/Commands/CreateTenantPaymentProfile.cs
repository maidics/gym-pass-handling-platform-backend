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
        var gymId = await _context
            .GymEmployments
            .AsNoTracking()
            .Where(x => x.UserId == _user.Id)
            .Select(x => x.GymId)
            .FirstOrDefaultAsync(cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymId, $"{nameof(GymEmployment)}.{nameof(GymEmployment.GymId)}", _user.Id);

        var paymentProfile = await _context
            .TenantPaymentProfiles
            .FirstOrDefaultAsync(x => x.GymId == gymId, cancellationToken);

        if (paymentProfile is not null)
        {
            return Result.Forbidden(
                _localizer.GetWithParamsLocalized(nameof(SharedResource.ResourceAlreadyExists), nameof(SharedResource.TenantPaymentProfile)));
        }

        var result = await _paymentTenantService
            .CreateTenantAccount(gymId, command.PaymentAccountHolderEmail, command.BusinessName, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ToFailure<PaymentProviderLinkDto>();
        }

        paymentProfile = new TenantPaymentProfile
        {
            GymId = gymId,
            PaymentAccountId = result.Value
        };

        await _context.TenantPaymentProfiles.AddAsync(paymentProfile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await _paymentTenantService.GenerateAccountLinkAsync(result.Value, gymId, true, cancellationToken: cancellationToken);
    }
}
