using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record SetupTenantPaymentProfileCommand(
    string PaymentAccountHolderEmail,
    string BusinessName
) : IRequest<Result<(string url, DateTime expirationDateTime)>>;

public class SetupTenantPaymentProfileCommandHandler : IRequestHandler<SetupTenantPaymentProfileCommand, Result<(string url, DateTime expirationDateTime)>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<SetupTenantPaymentProfileCommandHandler> _logger;
    private readonly IPaymentTenantService _paymentTenantService;

    public SetupTenantPaymentProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<SetupTenantPaymentProfileCommandHandler> logger,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _paymentTenantService = paymentTenantService;
    }

    public async Task<Result<(string url, DateTime expirationDateTime)>> Handle(SetupTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.ApplicationUserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            throw new SystemException(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var paymentProfile = await _context
            .TenantPaymentProfiles
            .FindAsync(gymEmployment.GymId);

        bool isOnboarding = paymentProfile is null;

        if (paymentProfile is null)
        {
            var creationResult = await _paymentTenantService
                .CreateTenantAccount(gymEmployment.GymId, command.PaymentAccountHolderEmail, command.BusinessName);

            if (!creationResult.Succeeded)
            {
                return creationResult.ToNewFailure<(string url, DateTime expirationDateTime)>();
            }

            paymentProfile = new TenantPaymentProfile
            {
                GymId = gymEmployment.GymId,
                TenantPaymentAccountId = creationResult.Value
            };

            await _context.TenantPaymentProfiles.AddAsync(paymentProfile);
        }

        var linkGenerationResult = await _paymentTenantService.GenerateAccountLinkAsync(
            paymentProfile.TenantPaymentAccountId!,
            "TODO: set return url",
            "TODO: set refresh url",
            isOnboarding);

        return Result<(string url, DateTime expirationDateTime)>
            .Success((linkGenerationResult.Value.url, linkGenerationResult.Value.expiration));
    }
}
