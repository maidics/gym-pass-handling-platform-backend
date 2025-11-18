/*
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

        var gym = await _context
            .Gyms
            .Include(g => g.PaymentProfile)
            .FirstOrDefaultAsync(g => g.Id == gymEmployment.GymId);

        Guard.Against.Null(gym, nameof(Gym));

        if (gym!.PaymentProfile is null)
        {
            var creationResult = await _paymentTenantService
                .CreateTenantAccount(gym.Id, command.PaymentAccountHolderEmail, command.BusinessName);

            if (!creationResult.Succeeded)
            {
                return Result<(string url, DateTime expirationDateTime)>
                    .Failure(creationResult.Errors, creationResult.Type);
            }

            var paymentProfile = new TenantPaymentProfile
            {
                GymId = gym.Id
            };
        }

        var result = await _paymentTenantService.CreateTenantAccount(
            gym.Id, 
            command.PaymentAccountHolderEmail, 
            command.BusinessName);

        if (!result.Succeeded)
        {
            return Result<(string url, DateTime expirationDateTime)>
                .Failure(["Failed to create payment account or generate onboarding link."], result.Type);
        }



        return Result<(string url, DateTime expirationDateTime)>
            .Success((result.Value.onboardingUrl, result.Value.expirationTime), result.Type);
    }
}
*/
