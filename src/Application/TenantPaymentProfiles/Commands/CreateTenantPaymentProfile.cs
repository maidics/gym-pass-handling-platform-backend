using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateTenantPaymentProfileCommand(
    string PaymentAccountHolderEmail,
    string BusinessName
) : IRequest<Result<(string url, DateTimeOffset expirationDateTime)>>;

public class CreateTenantPaymentProfileCommandValidator : AbstractValidator<CreateTenantPaymentProfileCommand>
{
    public CreateTenantPaymentProfileCommandValidator()
    {
        RuleFor(v => v.PaymentAccountHolderEmail).ValidEmailAddress(nameof(CreateTenantPaymentProfileCommand.PaymentAccountHolderEmail));

        RuleFor(v => v.BusinessName).NotEmptyWithMessage(nameof(CreateTenantPaymentProfileCommand.BusinessName));
    }
}

public class CreateTenantPaymentProfileCommandHandler : IRequestHandler<CreateTenantPaymentProfileCommand, Result<(string url, DateTimeOffset expirationDateTime)>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentTenantService _paymentTenantService;

    public CreateTenantPaymentProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _paymentTenantService = paymentTenantService;
    }

    public async Task<Result<(string url, DateTimeOffset expirationDateTime)>> Handle(CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
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
            return Result.Forbidden("Payment profile already exists.");
        }

        var result = await _paymentTenantService
            .CreateTenantAccount(gymEmployment.GymId, command.PaymentAccountHolderEmail, command.BusinessName);

        if (!result.Succeeded)
        {
            return result.ToFailure<(string url, DateTimeOffset expirationDateTime)>();
        }

        paymentProfile = new TenantPaymentProfile
        {
            GymId = gymEmployment.GymId,
            PaymentAccountId = result.Value
        };

        await _context.TenantPaymentProfiles.AddAsync(paymentProfile);
        await _context.SaveChangesAsync();

        return await _paymentTenantService.GenerateAccountLinkAsync(result.Value, true);
    }
}
