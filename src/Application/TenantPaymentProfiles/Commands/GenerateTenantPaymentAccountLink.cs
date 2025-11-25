using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.TenantPaymentProfiles.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record GenerateTenantPaymentAccountLinkCommand : IRequest<Result<(string url, DateTimeOffset expiration)>>;

public class GenerateTenantPaymentAccountLinkCommandHandler : IRequestHandler<GenerateTenantPaymentAccountLinkCommand, Result<(string url, DateTimeOffset expiration)>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GenerateTenantPaymentAccountLinkCommandHandler> _logger;
    private readonly IPaymentTenantService _paymentTenantService;

    public GenerateTenantPaymentAccountLinkCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GenerateTenantPaymentAccountLinkCommandHandler> logger,
        IPaymentTenantService paymentTenantService
    )
    {
        _context = context;
        _user = user;
        _logger = logger;
        _paymentTenantService = paymentTenantService;
    }

    public async Task<Result<(string url, DateTimeOffset expiration)>> Handle(GenerateTenantPaymentAccountLinkCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId != null && ge.UserId == _user.Id);

        if (gymEmployment is null)
        {
            LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(
                _logger,
                _user.Roles,
                _user.Id,
                nameof(GymEmployment));

            return Result.InternalError(ErrorMessages.AuthenticatedUserRelatedEntityNotFound(nameof(GymEmployment)));
        }

        var paymentProfile = await _context
            .TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tpp => tpp.GymId == gymEmployment.GymId);

        if (paymentProfile?.TenantPaymentAccountId is null)
        {
            return Result.BusinessRuleViolation("Gym has no payment profile.");
        }

        return await _paymentTenantService.GenerateAccountLinkAsync(paymentProfile.TenantPaymentAccountId, false);
    }
}