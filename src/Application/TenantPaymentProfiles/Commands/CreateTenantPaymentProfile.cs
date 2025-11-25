using FitPass.Application.Common.Exceptions;
using FitPass.Application.Common.Extensions;
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
public record CreateTenantPaymentProfileCommand(
    string PaymentAccountHolderEmail,
    string BusinessName
) : IRequest<Result<(string url, DateTime expirationDateTime)>>;

public class CreateTenantPaymentProfileCommandValidator : AbstractValidator<CreateTenantPaymentProfileCommand>
{
    public CreateTenantPaymentProfileCommandValidator()
    {
        RuleFor(v => v.PaymentAccountHolderEmail).ValidEmailAddress(nameof(CreateTenantPaymentProfileCommand.PaymentAccountHolderEmail));

        RuleFor(v => v.BusinessName).NotEmptyWithMessage(nameof(CreateTenantPaymentProfileCommand.BusinessName));
    }
}

public class CreateTenantPaymentProfileCommandHandler : IRequestHandler<CreateTenantPaymentProfileCommand, Result<(string url, DateTime expirationDateTime)>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<CreateTenantPaymentProfileCommandHandler> _logger;
    private readonly IPaymentTenantService _paymentTenantService;

    public CreateTenantPaymentProfileCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<CreateTenantPaymentProfileCommandHandler> logger,
        IPaymentTenantService paymentTenantService)
    {
        _context = context;
        _user = user;
        _logger = logger;
        _paymentTenantService = paymentTenantService;
    }

    public async Task<Result<(string url, DateTime expirationDateTime)>> Handle(CreateTenantPaymentProfileCommand command, CancellationToken cancellationToken)
    {
        var gymEmployment = await _context
            .GymEmployments
            .AsNoTracking()
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

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

        if (paymentProfile is not null)
        {
            throw new ForbiddenAccessException();
        }

        var creationResult = await _paymentTenantService
            .CreateTenantAccount(gymEmployment.GymId, command.PaymentAccountHolderEmail, command.BusinessName);

        if (!creationResult.Succeeded)
        {
            return creationResult.ToFailure<(string url, DateTime expirationDateTime)>();
        }

        paymentProfile = new TenantPaymentProfile
        {
            GymId = gymEmployment.GymId,
            TenantPaymentAccountId = creationResult.Value
        };

        await _context.TenantPaymentProfiles.AddAsync(paymentProfile);

        return await _paymentTenantService.GenerateAccountLinkAsync(creationResult.Value, true);
    }
}
