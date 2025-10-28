using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Logging;
using FitPass.Application.Common.Security;
using FitPass.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize]
public record DeleteMyAccountCommand : IRequest;

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IStripeCustomerService _stripeCustomerService;
    private readonly ILogger<DeleteMyAccountCommandHandler> _logger;
    private readonly IApplicationDbContext _context;

    public DeleteMyAccountCommandHandler(
        IIdentityService identityService,
        IUser user,
        IStripeCustomerService stripeCustomerService,
        Logger<DeleteMyAccountCommandHandler> logger,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _user = user;
        _stripeCustomerService = stripeCustomerService;
        _logger = logger;
        _context = context;
    }

    public async Task Handle(DeleteMyAccountCommand command, CancellationToken cancellationToken)
    {
        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            var result = await _identityService.DeleteUserAsync(_user.Id!);

            if (result.IsUserNotFoundFailure())
            {
                await transaction.RollbackAsync();

                LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, "ApplicationUser");

                throw new UnauthorizedAccessException();
            }

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                LogErrorMessages.IdentityServiceMethodFailed(_logger, nameof(IIdentityService.DeleteUserAsync), _user.Roles?[0], _user.Id, result);
                _logger.LogError("Failed to delete ({UserId}) user. IdentityResult: {IdentityResult}", _user.Id, result);
                throw new InvalidOperationException($"Failed to delete user.");
            }

            var paymentProfile = await _context
                .UserPaymentProfiles
                .FirstOrDefaultAsync(upp => upp.ApplicationUserId == _user.Id);

            if (paymentProfile == null)
            {
                await transaction.RollbackAsync();

                LogCriticalMessages.AuthenticatedUserRelatedEntityNotFound(_logger, _user.Roles, _user.Id, nameof(UserPaymentProfile));

                throw new Exception("Authenticated user's UserPaymentProfile not found.");
            }

            if (paymentProfile.StripeCustomerId != null)
            {
                await _stripeCustomerService.DeleteCustomerFromStripe(paymentProfile.StripeCustomerId);
            }

            _context.UserPaymentProfiles.Remove(paymentProfile);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception ex)
        {
            await transaction.RollbackAsync();

            if (ex.IsStripeServiceException())
            {
                throw;
            }

            LogErrorMessages.UnhandledExceptionCaught(_logger, nameof(DeleteMyAccountCommandHandler), ex);

            throw;
        }
    }
}
